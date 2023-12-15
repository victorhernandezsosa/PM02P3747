namespace PM02P3747
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(CrearNota), typeof(CrearNota));

        }
    }
}
