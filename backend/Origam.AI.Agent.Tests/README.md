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

## Lookup wizard

`LookupWizard_CreatesTheLookupAndItsDataStructure` asks the agent for a lookup on
`Dimension1` (display field `Name`, id filter `GetId`, no list filter) and checks that
both the lookup and its `Lookup<name>` data structure were persisted. Both are deleted
on teardown.

## Screen section widgets

The `Widget_*` tests check that the agent can put every screen section widget on a
section: text box, date box, check box, label, drop-down, color picker, blob control,
image box, group box, radio buttons, multi column adapter field wrapper, checklist and
tag input. The first of them to run asks the agent itself, in two turns, for a throwaway
entity (`AiWidget<hex>`) with a field of every needed type and for a child entity with one
relation and the two Array fields, and in a third turn for a screen section over that entity
(the wizard asks to confirm the field selection, so a fourth turn answers yes when needed);
it then checks through the Architect API that everything is there. The later widget tests
reuse that section, and the report shows the setup turns as one row, `WidgetSection_Setup`.
Each test asks the agent for one widget and reads the saved section back through
`SectionEditor/Update`. They need the lookup `LookupBusinessPartner` from the Root package
for the drop-down, checklist and tag input and are ignored when that lookup is missing.

```
dotnet test Origam.AI.Agent.Tests/bin/Debug/net8.0/Origam.AI.Agent.Tests.dll --filter "FullyQualifiedName~AgentIntegrationTests.Widget_" --logger "console;verbosity=detailed"
```
