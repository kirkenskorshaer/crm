namespace SystemInterface.DanskeBank
{
	public static class ResponseCode
	{
		public static ResponseCodeEnum GetResponse(string codeString)
		{
			int codeInt;
			int.TryParse(codeString, out codeInt);

			return (ResponseCodeEnum)codeInt;
		}

		public enum ResponseCodeEnum
		{
			OK = 00,
			Pending_Not_used = 01,
			SOAP_signature_error_Signature_verification_failed = 02,
			SOAP_signature_error_Certificate_not_valid_for_this_id = 03,
			SOAP_signature_error_Certificate_not_valid = 04,
			Operation_unknown = 05,
			Operation_is_restricted = 06,
			SenderID_not_found = 07,
			SenderID_locked = 08,
			Contract_locked = 09,
			SenderID_outdated = 10,
			Contract_outdated = 11,
			Schemavalidation_failed = 12,
			CustomerID_not_found = 13,
			CustomerID_locked = 14,
			CustomerID_outdated = 15,
			Product_contract_outdated = 16,
			Product_contract_locked = 17,
			Content_digital_signature_not_valid = 18,
			Content_certificate_not_valid = 19,
			Content_type_not_valid = 20,
			Deflate_error = 21,
			Decrypt_error = 22,
			Content_processing_error = 23,
			Content_not_found = 24,
			Content_not_allowed = 25,
			Technical_error = 26,
			Cannot_be_deleted = 27,
			Invalid_parameters = 29,
			Authentication_failed = 30,
			Duplicate_message_rejected_SOAP = 31,
			Duplicate_ApplicationRequestRejected_ApplicationRequest = 32,
		}
	}
}
