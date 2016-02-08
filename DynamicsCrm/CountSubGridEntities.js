function CountSubGridEntities(controlName, attributeName)
{
	var filteredRecordCount = Xrm.Page.getControl(controlName).getGrid().getTotalRecordCount();

	var countField = Xrm.Page.getAttribute(attributeName).getValue();

	if(filteredRecordCount != countField)
	{
		Xrm.Page.getAttribute(attributeName).setValue(filteredRecordCount);
	}

	setTimeout(function ()
	{
		CountSubGridEntities(controlName, attributeName);
	}, 1000);
}

function SetCount(controlName, attributeName)
{
	setTimeout(function ()
	{
		CountSubGridEntities(controlName, attributeName);
	}, 1000);
}