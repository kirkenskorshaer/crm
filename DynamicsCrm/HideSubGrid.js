function IndsamlereHide()
{
	var selectedOption = Xrm.Page.getAttribute("new_erindsamlingsleder").getText();

	if (selectedOption == "Yes")
	{
		Xrm.Page.getControl("Indsamlere").setVisible(true);
	}
	else
	{
		Xrm.Page.getControl("Indsamlere").setVisible(false);
	}
}