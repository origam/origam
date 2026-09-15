# Origam AI tests — the benchmark report

`AgentIntegrationTests` drives a real model through real Architect tools. Every run
records tool calls, tokens, wall time and cost, and prints them as a benchmark.

## Running

```
MSBuild.exe Origam.sln -t:"AI\Origam_AI_Agent_Tests" -p:Configuration="Release Architect Server"
vstest.console.exe Origam.AI.Agent.Tests/bin/Release/net8.0/Origam.AI.Agent.Tests.dll
```

The tests boot Architect in-process. Set `ORIGAM_ARCHITECT_URL` to run them against an
already running server instead.

They read the API key from the Architect server's configuration
(see `Origam.AI.Agent/README.md`). With no key every benchmark test calls
`Assert.Ignore` instead of failing.

The model is copied into a temporary sandbox for the run, so the tests never write into
the model in `OrigamSettings.config`. The sandbox is removed afterwards. With
`ORIGAM_ARCHITECT_URL` set there is no sandbox — that server writes into its own model.

## The report

Written on teardown by `BenchmarkReport.CompleteRun()`, in two forms.

Console table, one row per test plus a total:

```
ORIGAM AI AGENT BENCHMARK
model:   gpt-5.6-luna
backend: in-process Architect, package 'Api'
--------------------------------------------------------------
test                       tools   prompt  cached  output  sec  USD
```

HTML file, overwritten on every run:

```
backend/Origam.AI.Agent.Tests/benchmark-report.html
```
