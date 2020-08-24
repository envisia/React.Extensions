using System;
using Newtonsoft.Json;
using React;

namespace Envisia.React.Extensions
{
    public class EnvisiaReduxStoreFunctions : RenderFunctionsBase
    {
        public string StoreState { get; private set; }

        private string InitialStore { get; }
        private bool JSLoaded { get; set; }

        public EnvisiaReduxStoreFunctions(string rootReducerName, object storeModel)
        {
            var serialized = JsonConvert.SerializeObject(
                storeModel,
                EnvisiaReactConstants.JsonCamelCaseSerializerSettings);

            InitialStore = $"store = ReduxStore.createStore({rootReducerName}, {serialized});";
        }

        /// <summary>
        /// Implementation of PreRender
        /// </summary>
        /// <param name="executeJs"></param>
        public override void PreRender(Func<string, string> executeJs)
        {
            if (JSLoaded)
            {
                return;
            }

            executeJs(InitialStore);
            JSLoaded = true;
        }

        public override void PostRender(Func<string, string> executeJs)
        {
            var state = executeJs("JSON.stringify(store.getState())");
            StoreState = state;
        }
    }
}