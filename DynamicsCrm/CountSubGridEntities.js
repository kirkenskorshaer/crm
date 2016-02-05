function CountSubGridEntities(controlName, attributeName)
{
	var filteredRecordCount = Xrm.Page.getControl(controlName).getGrid().getTotalRecordCount();

	var countField = Xrm.Page.getAttribute(attributeName).getValue();

	alert(filteredRecordCount);
	alert(countField);

	if(filteredRecordCount != countField)
	{
		Xrm.Page.getAttribute(attributeName).setValue(filteredRecordCount);
	}
}

function CountIndsamlere()
{
	var controlName = "Indsamlere";
	var attributeName = "new_antalindsamlere";

	var filteredRecordCount = Xrm.Page.getControl(controlName).getGrid().getTotalRecordCount();

	var countField = Xrm.Page.getAttribute(attributeName).getValue();

	alert(filteredRecordCount);
	alert(countField);

	if (filteredRecordCount != countField)
	{
		Xrm.Page.getAttribute(attributeName).setValue(filteredRecordCount);
	}
}

function SetCountIndsamlere()
{
	Xrm.Page.getControl("Indsamlere").addOnLoad(CountSubGridEntities("Indsamlere", "new_antalindsamlere"));
}