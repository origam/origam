#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Mvp.Xml.Exslt;
using Origam.Rule.XsltFunctions;

namespace Origam.Rule.Xslt;

public class OrigamXsltContext : XsltContext
{
    private Dictionary<string, object> _xslFunctionsDict;
    private XsltContext _exslt;

    public static OrigamXsltContext Create(XmlNameTable nameTable, string transactionId)
    {
        var functionContainers = XsltFunctionContainerFactory.Create(transactionId);
        return new OrigamXsltContext(nameTable, functionContainers);
    }

    public OrigamXsltContext(
        XmlNameTable nt,
        IEnumerable<XsltFunctionsDefinition> xsltFunctionsDefinitions
    )
        : base((NameTable)nt)
    {
        _exslt = new ExsltContext(nt);
        _xslFunctionsDict = new Dictionary<string, object>();
        foreach (var xsltFunctionsDefinition in xsltFunctionsDefinitions)
        {
            AddNamespace(
                xsltFunctionsDefinition.NameSpacePrefix,
                xsltFunctionsDefinition.NameSpaceUri
            );
            _xslFunctionsDict.Add(
                xsltFunctionsDefinition.NameSpaceUri,
                xsltFunctionsDefinition.Container
            );
        }
    }

    public override bool Whitespace
    {
        get { return false; }
    }

    public override int CompareDocument(string baseUri, string nextbaseUri)
    {
        return 0;
    }

    public override bool PreserveWhitespace(XPathNavigator node)
    {
        return false;
    }

    public override IXsltContextFunction ResolveFunction(
        string prefix,
        string name,
        XPathResultType[] ArgTypes
    )
    {
        object functionContainer;
        String ns = LookupNamespace(prefix);
        if (ns != null)
        {
            if (_xslFunctionsDict.TryGetValue(ns, out functionContainer))
            {
                IXsltContextFunction func = null;
                func = GetExtensionMethod(ns, name, ArgTypes, functionContainer);
                if (func == null)
                {
                    throw new XsltException(String.Format("Unknown Xslt function {0}", name));
                }
                if (ArgTypes.Length < func.Minargs || func.Maxargs < ArgTypes.Length)
                {
                    throw new XsltException(
                        String.Format(
                            "Function {0} has wrong number of params {1}",
                            name,
                            ArgTypes.Length.ToString(CultureInfo.InvariantCulture)
                        )
                    );
                }
                return func;
            }
        }
        if (string.IsNullOrEmpty(prefix))
        {
            throw new Exception(
                $"Could not resolve xslt function \"{name}\" with namespace prefix \"{prefix}\""
            );
        }
        return _exslt.ResolveFunction(prefix, name, ArgTypes);
    }

    public override IXsltContextVariable ResolveVariable(string prefix, string name)
    {
        return null;
    }

    #region private functions
    private bool Equals(string a, string b, bool ignoreCase)
    {
        return string.Compare(
                a,
                b,
                ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal
            ) == 0;
    }

    private string GetNameFromAttribute(MethodInfo method)
    {
        var xsltFunctionAttribute =
            method.GetCustomAttribute(typeof(XsltFunctionAttribute)) as XsltFunctionAttribute;
        return xsltFunctionAttribute?.XsltName;
    }

    private MethodInfo FindBestMethod(
        MethodInfo[] methods,
        bool ignoreCase,
        bool publicOnly,
        string name,
        XPathResultType[] argTypes
    )
    {
        int length = methods.Length;
        int free = 0;
        // restrict search to methods with the same name and requested protection attribute
        for (int i = 0; i < length; i++)
        {
            if (
                Equals(name, methods[i].Name, ignoreCase)
                || Equals(name, GetNameFromAttribute(methods[i]), ignoreCase)
            )
            {
                if (!publicOnly || methods[i].GetBaseDefinition().IsPublic)
                {
                    methods[free++] = methods[i];
                }
            }
        }
        length = free;
        if (length == 0)
        {
            // this is the only place we returning null in this function
            return null;
        }
        if (argTypes == null)
        {
            // without arg types we can't do more detailed search
            return methods[0];
        }
        // restrict search by number of parameters
        free = 0;
        for (int i = 0; i < length; i++)
        {
            if (methods[i].GetParameters().Length == argTypes.Length)
            {
                methods[free++] = methods[i];
            }
        }
        length = free;
        if (length <= 1)
        {
            // 0 -- not method found. We have to return non-null and let it fail with corect exception on call.
            // 1 -- no reason to continue search anyway.
            return methods[0];
        }
        // restrict search by parameters type
        free = 0;
        for (int i = 0; i < length; i++)
        {
            bool match = true;
            ParameterInfo[] parameters = methods[i].GetParameters();
            for (int par = 0; par < parameters.Length; par++)
            {
                XPathResultType required = argTypes[par];
                if (required == XPathResultType.Any)
                {
                    continue; // Any means we don't know type and can't discriminate by it
                }
                XPathResultType actual = GetXPathType(parameters[par].ParameterType);
                if (
                    actual != required
                    && actual != XPathResultType.Any // actual arg is object and we can pass everithing here.
                )
                {
                    match = false;
                    break;
                }
            }
            if (match)
            {
                methods[free++] = methods[i];
            }
        }
        length = free;
        return methods[0];
    }

    private const BindingFlags bindingFlags =
        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

    private IXsltContextFunction GetExtensionMethod(
        string ns,
        string name,
        XPathResultType[] argTypes,
        object extension
    )
    {
        MethodInfo method = FindBestMethod(
            extension.GetType().GetMethods(bindingFlags), /*ignoreCase:*/
            false, /*publicOnly:*/
            true,
            name,
            argTypes
        );
        if (method != null)
        {
            return new FuncExtension(extension, method, null);
        }
        return null;
    }

    public static XPathResultType GetXPathType(Type type)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.String:
            {
                return XPathResultType.String;
            }
            case TypeCode.Boolean:
            {
                return XPathResultType.Boolean;
            }
            case TypeCode.Object:
            {
                if (
                    typeof(XPathNavigator).IsAssignableFrom(type)
                    || typeof(IXPathNavigable).IsAssignableFrom(type)
                )
                {
                    return XPathResultType.Navigator;
                }
                if (typeof(XPathNodeIterator).IsAssignableFrom(type))
                {
                    return XPathResultType.NodeSet;
                }
                // [....]: It be better to check that type is realy object and otherwise return XPathResultType.Error
                return XPathResultType.Any;
            }

            case TypeCode.DateTime:
            {
                return XPathResultType.Error;
            }
            default: /* all numeric types */
            {
                return XPathResultType.Number;
            }
        }
    }

    private class FuncExtension : XsltFunctionImpl
    {
        private object extension;
        private MethodInfo method;
        private TypeCode[] typeCodes;
        private PermissionSet permissions;

        public FuncExtension(object extension, MethodInfo method, PermissionSet permissions)
        {
            System.Diagnostics.Debug.Assert(extension != null);
            System.Diagnostics.Debug.Assert(method != null);
            this.extension = extension;
            this.method = method;
            this.permissions = permissions;
            XPathResultType returnType = GetXPathType(method.ReturnType);
            ParameterInfo[] parameters = method.GetParameters();
            int minArgs = parameters.Length;
            int maxArgs = parameters.Length;
            this.typeCodes = new TypeCode[parameters.Length];
            XPathResultType[] argTypes = new XPathResultType[parameters.Length];
            bool optionalParams = true; // we allow only last params be optional. Set false on the first non optional.
            for (int i = parameters.Length - 1; 0 <= i; i--)
            { // Revers order is essential: counting optional parameters
                typeCodes[i] = Type.GetTypeCode(parameters[i].ParameterType);
                argTypes[i] = GetXPathType(parameters[i].ParameterType);
                if (optionalParams)
                {
                    if (parameters[i].IsOptional)
                    {
                        minArgs--;
                    }
                    else
                    {
                        optionalParams = false;
                    }
                }
            }
            base.Init(minArgs, maxArgs, returnType, argTypes);
        }

        public override object Invoke(
            XsltContext xsltContext,
            object[] args,
            XPathNavigator docContext
        )
        {
            System.Diagnostics.Debug.Assert(
                args.Length <= this.Minargs,
                "We cheking this on resolve time"
            );
            for (int i = args.Length - 1; 0 <= i; i--)
            {
                args[i] = ConvertToXPathType(args[i], this.ArgTypes[i], this.typeCodes[i]);
            }
            if (this.permissions != null)
            {
                this.permissions.PermitOnly();
            }
            return method.Invoke(extension, args);
        }
    }

    private abstract class XsltFunctionImpl : IXsltContextFunction
    {
        private int minargs;
        private int maxargs;
        private XPathResultType returnType;
        private XPathResultType[] argTypes;

        public XsltFunctionImpl() { }

        public XsltFunctionImpl(
            int minArgs,
            int maxArgs,
            XPathResultType returnType,
            XPathResultType[] argTypes
        )
        {
            this.Init(minArgs, maxArgs, returnType, argTypes);
        }

        protected void Init(
            int minArgs,
            int maxArgs,
            XPathResultType returnType,
            XPathResultType[] argTypes
        )
        {
            this.minargs = minArgs;
            this.maxargs = maxArgs;
            this.returnType = returnType;
            this.argTypes = argTypes;
        }

        public int Minargs
        {
            get { return this.minargs; }
        }
        public int Maxargs
        {
            get { return this.maxargs; }
        }
        public XPathResultType ReturnType
        {
            get { return this.returnType; }
        }
        public XPathResultType[] ArgTypes
        {
            get { return this.argTypes; }
        }
        public abstract object Invoke(
            XsltContext xsltContext,
            object[] args,
            XPathNavigator docContext
        );

        // static helper methods:
        public static XPathNodeIterator ToIterator(object argument)
        {
            XPathNodeIterator it = argument as XPathNodeIterator;
            if (it == null)
            {
                throw new XsltException("Can't convert to nodeset");
            }
            return it;
        }

        public static XPathNavigator ToNavigator(object argument)
        {
            XPathNavigator nav = argument as XPathNavigator;
            if (nav == null)
            {
                throw new XsltException("Can't convert to navigator.");
            }
            return nav;
        }

        private static string IteratorToString(XPathNodeIterator it)
        {
            System.Diagnostics.Debug.Assert(it != null);
            if (it.MoveNext())
            {
                return it.Current.Value;
            }
            return string.Empty;
        }

        public static string ToString(object argument)
        {
            XPathNodeIterator it = argument as XPathNodeIterator;
            if (it != null)
            {
                return IteratorToString(it);
            }
            //return XmlConvert.ToXPathString(argument);
            return XmlTools.ConvertToString(argument);
        }

        public static bool ToBoolean(object argument)
        {
            XPathNodeIterator it = argument as XPathNodeIterator;
            if (it != null)
            {
                return Convert.ToBoolean(IteratorToString(it), CultureInfo.InvariantCulture);
            }
            XPathNavigator nav = argument as XPathNavigator;
            if (nav != null)
            {
                return Convert.ToBoolean(nav.ToString(), CultureInfo.InvariantCulture);
            }
            return Convert.ToBoolean(argument, CultureInfo.InvariantCulture);
        }

        public static double ToNumber(object argument)
        {
            XPathNodeIterator it = argument as XPathNodeIterator;
            if (it != null)
            {
                //return XmlConvert.ToXPathDouble(IteratorToString(it));
                return XmlConvert.ToDouble(IteratorToString(it));
            }
            XPathNavigator nav = argument as XPathNavigator;
            if (nav != null)
            {
                //return XmlConvert.ToXPathDouble(nav.ToString());
                return XmlConvert.ToDouble(nav.ToString());
            }
            //return XmlConvert.ToXPathDouble(argument);
            return XmlConvert.ToDouble(XmlTools.ConvertToString(argument));
        }

        private static object ToNumeric(object argument, TypeCode typeCode)
        {
            return Convert.ChangeType(ToNumber(argument), typeCode, CultureInfo.InvariantCulture);
        }

        public static object ConvertToXPathType(object val, XPathResultType xt, TypeCode typeCode)
        {
            switch (xt)
            {
                case XPathResultType.String:
                {
                    // Unfortunately XPathResultType.String == XPathResultType.Navigator (This is wrong but cant be changed in Everett)
                    // Fortunately we have typeCode hare so let's discriminate by typeCode
                    if (typeCode == TypeCode.String)
                    {
                        return ToString(val);
                    }

                    return ToNavigator(val);
                }

                case XPathResultType.Number:
                {
                    return ToNumeric(val, typeCode);
                }
                case XPathResultType.Boolean:
                {
                    return ToBoolean(val);
                }
                case XPathResultType.NodeSet:
                {
                    return ToIterator(val);
                }
                case XPathResultType.Any:
                case XPathResultType.Error:
                {
                    return val;
                }
                default:
                {
                    System.Diagnostics.Debug.Assert(false, "unexpected XPath type");
                    return val;
                }
            }
        }
    }
    #endregion
}
