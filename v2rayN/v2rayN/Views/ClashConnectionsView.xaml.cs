using ReactiveUI;
using System.Reactive.Disposables;
using v2rayN.ViewModels;

namespace v2rayN.Views
{
    
    
    
    public partial class ClashConnectionsView
    {
        public ClashConnectionsView()
        {
            InitializeComponent();
            ViewModel = new ClashConnectionsViewModel();

            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel, vm => vm.ConnectionItems, v => v.lstConnections.ItemsSource).DisposeWith(disposables);
                this.Bind(ViewModel, vm => vm.SelectedSource, v => v.lstConnections.SelectedItem).DisposeWith(disposables);

                this.BindCommand(ViewModel, vm => vm.ConnectionCloseCmd, v => v.menuConnectionClose).DisposeWith(disposables);
                this.BindCommand(ViewModel, vm => vm.ConnectionCloseAllCmd, v => v.menuConnectionCloseAll).DisposeWith(disposables);

                this.Bind(ViewModel, vm => vm.SortingSelected, v => v.cmbSorting.SelectedIndex).DisposeWith(disposables);
                this.BindCommand(ViewModel, vm => vm.ConnectionCloseAllCmd, v => v.btnConnectionCloseAll).DisposeWith(disposables);
                this.Bind(ViewModel, vm => vm.AutoRefresh, v => v.togAutoRefresh.IsChecked).DisposeWith(disposables);
            });
        }

        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel?.ClashConnectionClose(false);
        }
    }
}