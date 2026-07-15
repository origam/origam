#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Origam.Rule.Xslt;
using Origam.Schema.GuiModel;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.BI.XslFO
{
    public class XslFOReportService : IReportService
    {
        private const string DefaultRendererUrl = "http://xslfo:8080";
        private const int DefaultRendererTimeoutSeconds = 60;

        public object GetReport(
            Guid reportId,
            IXmlContainer data,
            string format,
            Hashtable parameters,
            string dbTransaction
        )
        {
            if (format != null && !string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(format),
                    format,
                    string.Format(Strings.XslFOFormatNotSupported, format)
                );
            }

            var report = ReportHelper.GetReportElement<XslFOReport>(reportId);
            parameters ??= new Hashtable();
            ReportHelper.PopulateDefaultValues(report, parameters);

            IDataDocument xmlDataDoc = ReportHelper.LoadOrUseReportData(
                report,
                data,
                parameters,
                dbTransaction
            );

            var persistence = ServiceManager.Services.GetService<IPersistenceService>();
            IXsltEngine transformer = new CompiledXsltEngine(persistence.SchemaProvider);

            var resultDoc = transformer.Transform(
                xmlDataDoc,
                report.XslFOTransformationId,
                parameters,
                transactionId: dbTransaction,
                outputStructure: null,
                validateOnly: false
            );

            return RenderPdfWithXslFOServer(resultDoc.Xml.OuterXml);
        }

        private static byte[] RenderPdfWithXslFOServer(string xslFoXml)
        {
            if (string.IsNullOrWhiteSpace(xslFoXml))
            {
                throw new InvalidOperationException(Strings.XslFOEmptyDocument);
            }

            var rendererUrl = GetRendererUrl();
            var renderUri = new Uri(rendererUrl, "/render");
            var timeout = GetRendererTimeout();

            using var client = new HttpClient { BaseAddress = rendererUrl, Timeout = timeout };

            using var content = new StringContent(
                xslFoXml,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                "application/xml"
            );

            HttpResponseMessage response;

            try
            {
                response = client.PostAsync(renderUri, content).GetAwaiter().GetResult();
            }
            catch (TaskCanceledException ex)
            {
                throw new TimeoutException(
                    string.Format(Strings.XslFORendererTimeout, timeout.TotalSeconds, renderUri),
                    ex
                );
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException(
                    string.Format(Strings.XslFORendererCallFailed, renderUri),
                    ex
                );
            }

            using (response)
            {
                var responseBytes = response
                    .Content.ReadAsByteArrayAsync()
                    .GetAwaiter()
                    .GetResult();

                if (!response.IsSuccessStatusCode)
                {
                    var responseText = TryDecodeResponse(responseBytes);

                    throw new InvalidOperationException(
                        string.Format(
                            Strings.XslFORendererFailed,
                            renderUri,
                            (int)response.StatusCode,
                            response.ReasonPhrase,
                            responseText
                        )
                    );
                }

                if (responseBytes.Length == 0)
                {
                    throw new InvalidOperationException(
                        string.Format(Strings.XslFORendererEmptyResponse, renderUri)
                    );
                }

                return responseBytes;
            }
        }

        private static Uri GetRendererUrl()
        {
            var configuredUrl = Environment.GetEnvironmentVariable("XSLFO_RENDERER_URL");

            if (string.IsNullOrWhiteSpace(configuredUrl))
            {
                configuredUrl = DefaultRendererUrl;
            }

            configuredUrl = configuredUrl.Trim();

            if (!configuredUrl.EndsWith("/", StringComparison.Ordinal))
            {
                configuredUrl += "/";
            }

            if (!Uri.TryCreate(configuredUrl, UriKind.Absolute, out var uri))
            {
                throw new InvalidOperationException(
                    string.Format(Strings.XslFORendererInvalidUrl, configuredUrl)
                );
            }

            return uri;
        }

        private static TimeSpan GetRendererTimeout()
        {
            var value = Environment.GetEnvironmentVariable("XSLFO_RENDERER_TIMEOUT_SECONDS");

            if (int.TryParse(value, out var seconds) && seconds > 0)
            {
                return TimeSpan.FromSeconds(seconds);
            }

            return TimeSpan.FromSeconds(DefaultRendererTimeoutSeconds);
        }

        private static string TryDecodeResponse(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return string.Empty;
            }

            try
            {
                return Encoding.UTF8.GetString(bytes);
            }
            catch (DecoderFallbackException)
            {
                return string.Format(Strings.XslFORendererDecodeFailed, bytes.Length);
            }
        }

        public string PrepareExternalReportViewer(
            Guid reportId,
            IXmlContainer data,
            string format,
            Hashtable parameters,
            string dbTransaction
        )
        {
            throw new NotImplementedException();
        }

        public void PrintReport(
            Guid reportId,
            IXmlContainer data,
            string printerName,
            int copies,
            Hashtable parameters
        )
        {
            throw new NotImplementedException();
        }

        public void SetTraceTaskInfo(TraceTaskInfo traceTaskInfo) { }
    }
}
