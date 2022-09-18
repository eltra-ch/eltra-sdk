using Xamarin.Forms;
using EltraUiCommon.Controls;
using EltraXamCommon.Framework;
using EltraXamCommon.Plugins.Events;
using System;
using EltraXamCommon.Dialogs;
using Prism.Services.Dialogs;
using EltraXamCommon.Plugins;

namespace EltraXamCommon.Controls
{
    public class XamToolViewModel : ToolViewModel
    {
        #region

        private DialogRequestedEventArgs _dialogRequestedEventArgs;
        private IEltraNavigoPluginService _pluginService;

        #endregion

        #region Constructors

        public XamToolViewModel()
        {
            Init(new InvokeOnMainThread());
        }

        public XamToolViewModel(ToolViewBaseModel parent)
            : base(parent)
        {
            Init(new InvokeOnMainThread());

            if(parent is XamToolViewModel xamTool)
            {
                PluginService = xamTool.PluginService;
            }
        }

        #endregion

        #region Properties

        public IEltraNavigoPluginService PluginService
        {
            get => _pluginService;
            set
            {
                _pluginService = value;
                OnPluginServiceChanged();
            }
        }

        public ImageSource Image { get; set; }

        protected DialogRequestedEventArgs DialogRequestedEventArgs
        {
            get => _dialogRequestedEventArgs ?? (_dialogRequestedEventArgs = CreateDialogRequestedEventArgs());
        }

        #endregion

        #region Events

        public event EventHandler<DialogRequestedEventArgs> DialogRequested;

        #endregion

        #region Events handling

        protected void OnDialogRequested(XamDialogViewModel viewModel, IDialogParameters parameters)
        {
            DialogRequestedEventArgs.Parameters = parameters;
            DialogRequestedEventArgs.ViewModel = viewModel;

            DialogRequested?.Invoke(this, DialogRequestedEventArgs);
        }

        private void OnPluginServiceChanged()
        {
            foreach(var child in SafeChildrenArray)
            {
                if(child is XamToolViewModel childViewModel)
                {
                    childViewModel.PluginService = PluginService;
                }
            }
        }

        #endregion

        #region Methods

        protected void ShowDialog(XamDialogViewModel viewModel, IDialogParameters parameters)
        {
            DialogRequestedEventArgs.ViewModel = viewModel;
            DialogRequestedEventArgs.Parameters = parameters;

            PluginService.ShowDialog(this, DialogRequestedEventArgs);
        }

        private DialogRequestedEventArgs CreateDialogRequestedEventArgs()
        {
            var result = new DialogRequestedEventArgs();

            result.PropertyChanged += (s, e) => 
            {
                if(e.PropertyName == "DialogResult")
                {
                    if(s is DialogRequestedEventArgs args)
                    {
                        if(args.Sender is XamToolViewModel viewModel)
                        {
                            viewModel.OnDialogClosed(viewModel, args.DialogResult);
                        }
                        else
                        {
                            OnDialogClosed(args.ViewModel, args.DialogResult);
                        }
                    }
                }
            };

            return result;
        }

        protected virtual void OnDialogClosed(object sender, IDialogResult dialogResult)
        {
        }

        #endregion
    }
}
