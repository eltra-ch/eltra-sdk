﻿using System;
using System.Collections.Generic;
using EltraNavigo.Controls;
using EltraNavigo.Views.About;
using EltraNavigo.Views.Login;
using System.Threading.Tasks;
using EltraNavigo.Views.Contact;

namespace EltraNavigo.Views
{
    public class MasterViewModel : BaseViewModel
    {
        #region Private fields
        
        private List<ToolViewModel> _viewModels;
        private List<ToolViewModel> _headerViewModels;
        private List<ToolViewModel> _toolViewModels;
        private List<ToolViewModel> _footerViewModels;
        private List<ToolViewModel> _supportedViewModels;

        private SignInViewModel _signInViewModel;
        private SignUpViewModel _signUpViewModel;

        private ContactViewModel _contactViewModel;

        private AboutViewModel _aboutViewModel;
        
        private ToolViewModel _activeViewModel;
        private ToolViewModel _previousViewModel;

        #endregion

        #region Constructors

        public MasterViewModel(BaseViewModel parent)
            : base(parent)
        {
            AddModels();
        }

        #endregion

        #region Events

        public event EventHandler PageChanged;

        #endregion

        #region Event handling

        protected virtual void OnPageChanged()
        {
            PageChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        public string LastUsedPageName
        {
            get
            {
                string result = string.Empty;
                var app = Xamarin.Forms.Application.Current;

                if (app.Properties.ContainsKey("last_used_page"))
                { 
                    result = app.Properties["last_used_page"] as string;
                }
                
                return result;
            }
            set
            {
                var app = Xamarin.Forms.Application.Current;

                app.Properties["last_used_page"] = value;
            }             
        }

        public ToolViewModel LastUsedPage
        {
            get
            {
                ToolViewModel result = SignInViewModel;

                foreach (var page in _viewModels)
                {
                    if( page.Uuid == LastUsedPageName)
                    {
                        result = page;
                        break;
                    }
                }

                return result;
            }
        }

        public List<ToolViewModel> ViewModels
        {
            get => _viewModels ?? (_viewModels = new List<ToolViewModel>());
            set => SetProperty(ref _viewModels, value);
        }

        public List<ToolViewModel> SupportedViewModels
        {
            get => _supportedViewModels ?? (_supportedViewModels = new List<ToolViewModel>());
            set => SetProperty(ref _supportedViewModels, value);
        }

        public List<ToolViewModel> HeaderViewModels
        {
            get => _headerViewModels ?? (_headerViewModels = new List<ToolViewModel>());
            set => SetProperty(ref _headerViewModels, value);
        }

        public List<ToolViewModel> ToolViewModels
        {
            get => _toolViewModels ?? (_toolViewModels = new List<ToolViewModel>());
            set => SetProperty(ref _toolViewModels, value);
        }

        public List<ToolViewModel> FooterViewModels
        {
            get => _footerViewModels ?? (_footerViewModels = new List<ToolViewModel>());
            set => SetProperty(ref _footerViewModels, value);
        }

        public ToolViewModel ActiveViewModel
        {
            get => _activeViewModel;
            set => SetProperty(ref _activeViewModel, value);
        }

        public SignInViewModel SignInViewModel => _signInViewModel ?? (_signInViewModel = CreateSignInViewModel());

        public SignUpViewModel SignUpViewModel => _signUpViewModel ?? (_signUpViewModel = CreateSignUpViewModel());

        public ContactViewModel ContactViewModel => _contactViewModel ?? (_contactViewModel = new ContactViewModel());

        public AboutViewModel AboutViewModel => _aboutViewModel ?? (_aboutViewModel = new AboutViewModel());

        #endregion

        #region Methods

        public void GotoFirstPage()
        {
            if (!SignInViewModel.AutoLogOnActive)
            {
                ChangePage(SignInViewModel, true);
            }
            else
            {
                ChangePage(SignInViewModel, true);
            }
        }

        private void GotoLastUsedPage()
        {
            if(LastUsedPage.IsSupported)
            {
                ChangePage(LastUsedPage, true);
            }
        }

        public async void ChangePage(ToolViewModel viewModel, bool internalChange = false)
        {
            if (viewModel != null && _activeViewModel != viewModel)
            {
                _previousViewModel = _activeViewModel;

                if (_previousViewModel != null)
                {
                    try
                    {
                        await _previousViewModel.Hide();
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                try
                {
                    await viewModel.Show();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                ActiveViewModel = viewModel;

                if(!internalChange)
                { 
                    LastUsedPageName = viewModel.Uuid;
                }

                Title = ActiveViewModel.Title;

                OnPageChanged();
            }
        }
        
        private SignInViewModel CreateSignInViewModel()
        {
            var result = new SignInViewModel();

            result.SignUpRequested += (sender, args) =>
            {
                ChangePage(SignUpViewModel, true);

                SignUpViewModel.LoginName = SignInViewModel.LoginName;
                SignUpViewModel.Password = SignInViewModel.Password;
            };

            result.Changed += (sender, args) =>
            {
                ActivateTools(true);

                ChangePage(ContactViewModel, true);
            };

            result.Canceled += (sender, args) =>
            {
                //ChangePage(_previousViewModel ?? DeviceListViewModel, true);
            };

            return result;
        }

        private SignUpViewModel CreateSignUpViewModel()
        {
            var result = new SignUpViewModel();

            result.Changed += (sender, args) =>
            {
                ActivateTools(true);

                ChangePage(ContactViewModel, true);
            };

            result.Canceled += (sender, args) =>
            {
                ChangePage(_previousViewModel ?? SignInViewModel, true);
            };

            return result;
        }

        private void ActivateTools(bool activate)
        {
            var supportedViewModels = new List<ToolViewModel>();

            supportedViewModels.AddRange(HeaderViewModels);

            foreach (var toolViewModel in ToolViewModels)
            {
                if (!supportedViewModels.Contains(toolViewModel))
                {
                    supportedViewModels.Add(toolViewModel);
                }
            }

            supportedViewModels.AddRange(FooterViewModels);

            SupportedViewModels = supportedViewModels;
        }


        private void AddModels()
        {
            ToolViewModels = new List<ToolViewModel>
            {
               ContactViewModel
            };

            HeaderViewModels = new List<ToolViewModel>
            {
                SignInViewModel,
                SignUpViewModel
            };

            FooterViewModels = new List<ToolViewModel>
            {
                AboutViewModel
            };

            var viewModels = new List<ToolViewModel>();
            
            viewModels.AddRange(HeaderViewModels);
            viewModels.AddRange(ToolViewModels);
            viewModels.AddRange(FooterViewModels);

            ViewModels = viewModels;

            var supportedViewModels = new List<ToolViewModel>();

            supportedViewModels.AddRange(HeaderViewModels);
            supportedViewModels.AddRange(FooterViewModels);

            SupportedViewModels = supportedViewModels;

            GotoFirstPage();
        }
        
        public async Task StopUpdate()
        {
            foreach (var viewModel in SupportedViewModels)
            {
                await viewModel.StopUpdate();
            }    
        }

        public async Task StartUpdate()
        {
            foreach (var viewModel in SupportedViewModels)
            {
                await viewModel.StartUpdate();
            }
        }

        #endregion        
    }
}
