$SEL = Select-String -Path *.trx -Pattern 'ResultSummary outcome="Failed"'
if ($SEL -ne $null)
{
    echo Contains Fail test
	exit 1
}