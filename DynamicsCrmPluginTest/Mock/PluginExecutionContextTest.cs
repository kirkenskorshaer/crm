using Microsoft.Xrm.Sdk;
using System;

namespace DynamicsCrmPluginTest.Mock
{
	public class PluginExecutionContextTest : IPluginExecutionContext
	{
		public Guid BusinessUnitId
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public Guid CorrelationId
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int Depth
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public Guid InitiatingUserId
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		private ParameterCollection _inputParameters = new ParameterCollection();
        public ParameterCollection InputParameters
		{
			get
			{
				return _inputParameters;
			}
		}

		public bool IsExecutingOffline
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool IsInTransaction
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool IsOfflinePlayback
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int IsolationMode
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public string MessageName
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int Mode
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public DateTime OperationCreatedOn
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public Guid OperationId
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public Guid OrganizationId
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public string OrganizationName
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		private ParameterCollection _outputParameters = new ParameterCollection();
        public ParameterCollection OutputParameters
		{
			get
			{
				return _outputParameters;
			}
		}

		public EntityReference OwningExtension
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public IPluginExecutionContext ParentContext
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public EntityImageCollection PostEntityImages
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public EntityImageCollection PreEntityImages
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public Guid PrimaryEntityId
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public string PrimaryEntityName
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public Guid? RequestId
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public string SecondaryEntityName
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public ParameterCollection SharedVariables
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int Stage
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public Guid UserId
		{
			get
			{
				throw new NotImplementedException();
			}
		}
	}
}
