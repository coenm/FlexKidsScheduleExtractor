namespace FlexKids.AzureFunction
{
    using System;
    using Microsoft.Extensions.Configuration;

    public class EnvironmentSetting
    {
        private readonly Action<IConfigurationBuilder> _builderAction;

        public EnvironmentSetting(Action<IConfigurationBuilder> builderAction)
        {
            _builderAction = builderAction;
        }

        public void BuilderAction(IConfigurationBuilder configurationBuilder)
        {
            _builderAction?.Invoke(configurationBuilder);
        }
    }
}