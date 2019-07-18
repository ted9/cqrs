using System.Collections;

namespace Cqrs.Contexts
{
    internal class OperationContext : CurrentContext
    {
        public OperationContext(IContextManager factory)
            : base(factory) 
        { }

		private static WcfStateExtension WcfOperationState
		{
			get
			{
				var extension = System.ServiceModel.OperationContext.Current.Extensions.Find<WcfStateExtension>();

				if (extension == null)
				{
					extension = new WcfStateExtension();
					System.ServiceModel.OperationContext.Current.Extensions.Add(extension);
				}

				return extension;
			}
        }


        protected override IDictionary GetMap()
		{
			return WcfOperationState.Map;
		}

        protected override void SetMap(IDictionary value)
		{
			WcfOperationState.Map = value;
		}


        class WcfStateExtension : System.ServiceModel.IExtension<System.ServiceModel.OperationContext>
        {
            public IDictionary Map { get; set; }

            public void Attach(System.ServiceModel.OperationContext owner) { }
            public void Detach(System.ServiceModel.OperationContext owner) { }
        }
    }
}
