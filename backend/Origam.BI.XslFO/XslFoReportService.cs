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
    public class XslFoReportService : IReportService
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
            if (
                !string.IsNullOrWhiteSpace(format)
                && !string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase)
            )
            {
                throw new NotSupportedException(
                    $"XSL-FO report format '{format}' is not supported. Only 'pdf' is supported."
                );
            }

            var report = ReportHelper.GetReportElement<XslFoReport>(reportId);
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
                report.XslFoTransformationId,
                parameters,
                transactionId: dbTransaction,
                outputStructure: null,
                validateOnly: false
            );

            return RenderPdfWithXslFoServer(resultDoc.Xml.OuterXml);
        }

        private static byte[] RenderPdfWithXslFoServer(string xslFoXml)
        {
            if (string.IsNullOrWhiteSpace(xslFoXml))
            {
                throw new InvalidOperationException(
                    "The XSL-FO transformation produced an empty document."
                );
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
                    $"XSL-FO renderer did not respond within {timeout.TotalSeconds} seconds. Renderer URL: {renderUri}",
                    ex
                );
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException(
                    $"Could not call XSL-FO renderer at '{renderUri}'.",
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
                        "XSL-FO renderer failed."
                            + Environment.NewLine
                            + "Renderer URL: "
                            + renderUri
                            + Environment.NewLine
                            + "HTTP status: "
                            + (int)response.StatusCode
                            + " "
                            + response.ReasonPhrase
                            + Environment.NewLine
                            + "Response:"
                            + Environment.NewLine
                            + responseText
                    );
                }

                if (responseBytes.Length == 0)
                {
                    throw new InvalidOperationException(
                        $"XSL-FO renderer returned an empty PDF response. Renderer URL: {renderUri}"
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
                    $"Invalid XSL-FO renderer URL '{configuredUrl}'. Check XSLFO_RENDERER_URL."
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
                return "<Could not decode renderer response as UTF-8. Response length: "
                    + bytes.Length
                    + " bytes.>";
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
