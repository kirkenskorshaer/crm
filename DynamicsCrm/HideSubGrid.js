function HideSubGridIfOptionSelected(selectedAttribute, showOptionValue, subGridToHide)
{
	var selectedOption = Xrm.Page.getAttribute(selectedAttribute).getText();

	if (selectedOption == showOptionValue)
	{
		Xrm.Page.getControl(subGridToHide).setVisible(true);
	}
	else
	{
		Xrm.Page.getControl(subGridToHide).setVisible(false);
	}
}