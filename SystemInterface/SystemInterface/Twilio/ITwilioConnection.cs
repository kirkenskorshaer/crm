namespace SystemInterface.Twilio
{
	public interface ITwilioConnection
	{
		MessageInfo Send(string toNumber, string messageBody);
	}
}
