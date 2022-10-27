$SEL = Select-String -Path TestResults\*.trx -Pattern 'ResultSummary outcome="Failed"'
if ($SEL -ne $null)
{
    echo Contains Fail test
	exit 1
}