#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using System.Collections;
using System.Reflection;
using System.IO;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;

using Microsoft.CSharp;

using Origam;
using Origam.DA.ObjectPersistence;
using Origam.Workbench.Services;

using Origam.Schema;
using Origam.Schema.EntityModel;

namespace OrigamArchitect
{
	/// <summary>
	/// Summary description for SchemaAssemblyGenerator.
	/// </summary>
	public class SchemaAssemblyGenerator
	{
		SchemaService _schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
		private string _getMethodPrefix = "GetId_";
		private string _persistenceProviderPropertyName = "PersistenceProvider";

		public SchemaAssemblyGenerator()
		{
		}

		public void WriteCode()
		{
			FunctionSchemaItemProvider functions = _schema.GetProvider(typeof(FunctionSchemaItemProvider)) as FunctionSchemaItemProvider;

			CSharpCodeProvider codeProvider = new CSharpCodeProvider();
			StringBuilder stringBuilder = new StringBuilder();
			StringWriter stringWriter = new StringWriter(stringBuilder);
			StreamWriter writer = new StreamWriter(@"C:\Documents and Settings\tvavrda.AS\Dokumenty\Visual Studio Projects\ORIGAM\OrigamArchitect\test.cs");

			CodeGeneratorOptions options = new CodeGeneratorOptions();
			
			options.BracingStyle = "C";

			ICodeGenerator gen = codeProvider.CreateGenerator();

			// namespaces
			CodeCompileUnit ccu = new CodeCompileUnit();
			CodeNamespace cns = new CodeNamespace("TestNamespace");
			ccu.Namespaces.Add(cns);

			cns.Imports.Add(new CodeNamespaceImport("Origam.Schema"));

			// class declaration
			CodeTypeDeclaration ctd=new CodeTypeDeclaration("Test");
			ctd.BaseTypes.Add(typeof(ICompiledModel));
			cns.Types.Add(ctd);

			// constructor
			CodeConstructor constructor=new CodeConstructor();
			constructor.Attributes=MemberAttributes.Public;
			ctd.Members.Add(constructor);

			// PersistenceProvider property implementation
			ctd.Members.Add(new CodeMemberField(typeof(IPersistenceProvider), "_persistenceProvider"));
			ctd.Members.Add(BuildProperty(typeof(IPersistenceProvider), _persistenceProviderPropertyName, "_persistenceProvider", true, true));

			// RetrieveItem Method implementation
			CodeMemberMethod factoryMethod = new CodeMemberMethod();
			factoryMethod.Name = "RetrieveItem";
			factoryMethod.ReturnType = new CodeTypeReference(typeof(object));
			factoryMethod.Attributes = MemberAttributes.Public;
			factoryMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "schemaItemId"));
			ctd.Members.Add(factoryMethod);


			Hashtable firstLetterCache = new Hashtable(16);

			foreach(ISchemaItem item in functions.PersistenceProvider.RetrieveList(typeof(AbstractSchemaItem), ""))
			{
				if(!(item is XslTransformation))
				{
					string id = item.PrimaryKey["Id"].ToString();
//					string firstLetter  = id.Substring(0,1);
//
//					CodeMemberMethod firstLetterMethod;
//					if(firstLetterCache.ContainsKey(firstLetter))
//					{
//						firstLetterMethod = firstLetterCache[firstLetter] as CodeMemberMethod;
//					}
//					else
//					{
//						firstLetterMethod = SwitchMethod(firstLetter);
//
//						CodeConditionStatement firstLetterCondition = new CodeConditionStatement(
//							new CodeBinaryOperatorExpression(
//								new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("schemaItemId"), "Substring", new CodeExpression[] {new CodePrimitiveExpression(0), new CodePrimitiveExpression(1)}) , 
//								CodeBinaryOperatorType.ValueEquality, 
//								new CodePrimitiveExpression(firstLetter)),
//							new CodeMethodReturnStatement(
//								new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), firstLetterMethod.Name),
//								new CodeExpression[] {new CodeVariableReferenceExpression("schemaItemId")}
//							)));
//
//						factoryMethod.Statements.Add(firstLetterCondition);
//						firstLetterCache.Add(firstLetter, firstLetterMethod);
//					}
//
//					CodeConditionStatement condition = new CodeConditionStatement(
//						new CodeBinaryOperatorExpression(
//							new CodeVariableReferenceExpression("id"), 
//							CodeBinaryOperatorType.ValueEquality, 
//							new CodePrimitiveExpression(id)),
//						new CodeMethodReturnStatement(
//							new CodeMethodInvokeExpression(
//							new CodeMethodReferenceExpression(
//								new CodeThisReferenceExpression(),
//								_getMethodPrefix + GetCodeId((Guid)item.PrimaryKey["Id"])
//						))));
//
//					firstLetterMethod.Statements.Add(condition);
					ctd.Members.Add(SchemaItemBuildMethod(item, cns));
				}
			}

			factoryMethod.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));

			foreach(CodeMemberMethod m in firstLetterCache.Values)
			{
				m.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
				ctd.Members.Add(m);
			}

			gen.GenerateCodeFromCompileUnit(ccu, writer, options);

			writer.Close();
			//stringWriter.Close();

			//System.Diagnostics.Debug.WriteLine(stringBuilder.ToString());
		}

		private string GetCodeId(Guid id)
		{
			return id.ToString().Replace("-", "_");
		}

		private CodeObjectCreateExpression GetGuidExpression(string id)
		{
			return new CodeObjectCreateExpression(typeof(Guid), new CodeExpression[] {new CodePrimitiveExpression(id)});
		}

		private CodeExpression ConvertValue(object value)
		{
			if(value == null)
			{
				return new CodePrimitiveExpression(null);
			}

			if(value.GetType().IsEnum)
			{
				return new CodeFieldReferenceExpression(
					new CodeTypeReferenceExpression(value.GetType()),
					value.ToString()
					);
			}
			else if(value is Guid)
			{
				return GetGuidExpression(value.ToString());
			}
			else
			{
				return new CodePrimitiveExpression(value);
			}
		}

		private CodeMemberMethod SwitchMethod(string id)
		{
			CodeMemberMethod method = new CodeMemberMethod();
			method.Name = "Get" + id;
			method.ReturnType = new CodeTypeReference("ISchemaItem");
			method.Attributes = MemberAttributes.Private;
			method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "id"));

			return method;
		}

		private CodeMemberMethod SchemaItemBuildMethod(ISchemaItem item, CodeNamespace nms)
		{
			string resultVarName = "result";
			string keyVarName = "key";

			nms.Imports.Add(new CodeNamespaceImport(item.GetType().Namespace));

			// private ISchemaItem GetId_f86ebad9_d9c3_4517_81d2_7f49ea499dff()
			CodeMemberMethod method = new CodeMemberMethod();
			method.Name = _getMethodPrefix + GetCodeId((Guid)item.PrimaryKey["Id"]);
			method.ReturnType = new CodeTypeReference("ISchemaItem");
			method.Attributes = MemberAttributes.Private;

			//ModelElementKey key = new ModelElementKey(id, schema);
			CodeVariableDeclarationStatement keyVariable = 
				new CodeVariableDeclarationStatement(
				typeof(ModelElementKey),
				keyVarName,
				new CodeObjectCreateExpression(typeof(ModelElementKey), 
				new CodeExpression[] {GetGuidExpression(item.PrimaryKey["Id"].ToString())})
				);

			// ReturnType result = new ReturnType(primaryKey);
			CodeVariableDeclarationStatement resultVariable = 
				new CodeVariableDeclarationStatement(
				item.GetType(),
				resultVarName,
				new CodeObjectCreateExpression(item.GetType(), 
				new CodeExpression[] {new CodeVariableReferenceExpression(keyVarName)})
				);
			
			// result.PersistenceProvider = this.PersistenceProvider
			CodeAssignStatement persistenceProviderAssignment = 
				new CodeAssignStatement(
				new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(resultVarName), "PersistenceProvider"),
				new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), _persistenceProviderPropertyName)
				);

			method.Statements.Add(keyVariable);
			method.Statements.Add(resultVariable);
			method.Statements.Add(persistenceProviderAssignment);


			// result.AttributeName = value
			IList members = Reflector.FindMembers(item.GetType(), typeof(EntityColumnAttribute), new Type[0]);
			foreach(MemberAttributeInfo mi in members)
			{
				object value = null;
				bool canWrite = true;

				if(mi.MemberInfo is PropertyInfo)
				{
					PropertyInfo pi = mi.MemberInfo as PropertyInfo;
					canWrite = pi.CanWrite;
					
					if(canWrite)
					{
						value = pi.GetValue(item, new object[0]);
					}
				}
				else
				{
					FieldInfo fi = mi.MemberInfo as FieldInfo;
					value = fi.GetValue(item);
				}

				EntityColumnAttribute column = mi.Attribute as EntityColumnAttribute;

				if(canWrite && !column.IsForeignKey)
				{
					CodeAssignStatement assign = new CodeAssignStatement(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(resultVarName), mi.MemberInfo.Name),
						ConvertValue(value)
						);

					method.Statements.Add(assign);
				}
			}

			// return result;
			CodeMethodReturnStatement returnStatement = 
				new CodeMethodReturnStatement(
				new CodeVariableReferenceExpression(resultVarName)
				);

			method.Statements.Add(returnStatement);

			return method;
		}

		private CodeMemberProperty BuildProperty(Type type, string propertyName, string fieldName, bool hasGet, bool hasSet)
		{
			CodeMemberProperty property = new CodeMemberProperty();
			property.Name = propertyName;
			property.Attributes = MemberAttributes.Public;
			property.HasGet = hasGet;
			property.HasSet = hasSet;
			property.Type = new CodeTypeReference(type);

			if(hasGet)
			{
				property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName)));
			}

			if(hasSet)
			{
				property.SetStatements.Add(new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName),
					new CodeVariableReferenceExpression("value")
					));
			}

			return property;
		}

	}
}