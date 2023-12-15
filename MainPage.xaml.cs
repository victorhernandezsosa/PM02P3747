using CommunityToolkit.Maui.Views;
using Firebase.Database;
using Firebase.Database.Query;
using Plugin.Maui.Audio;
using PM02P3747.Models;
using System.Collections.ObjectModel;

namespace PM02P3747
{
    public partial class MainPage : ContentPage
    {
        FirebaseClient client = new FirebaseClient("https://examenpm02-default-rtdb.firebaseio.com/");
        public ObservableCollection<Notas> Lista { get; set; } = new ObservableCollection<Notas>();

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;


            ListaDatos();
        }

        private void ListaDatos()
        {
            client.Child("Notas")
                .AsObservable<Notas>()
                .Subscribe((nota) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (nota.EventType == Firebase.Database.Streaming.FirebaseEventType.Delete)
                        {
                            var notaToDelete = Lista.FirstOrDefault(e => e.IdNota == nota.Object.IdNota);
                            if (notaToDelete != null)
                            {
                                Lista.Remove(notaToDelete);
                            }
                        }
                        else if (nota.Object != null)
                        {
                            var existingNota = Lista.FirstOrDefault(e => e.IdNota == nota.Object.IdNota);
                            if (existingNota != null)
                            {
                                Lista.Remove(existingNota);
                            }

                            Lista.Add(nota.Object);

                            // Ordenar la lista por antigüedad después de agregar una nueva nota
                            Lista = new ObservableCollection<Notas>(Lista.OrderByDescending(n => n.Fecha));

                            // Actualizar la propiedad ItemsSource de listaCollection después de cambios en Lista
                            listaCollection.ItemsSource = Lista.ToList();
                        }
                    });
                });
        }

        private async void nuevanota_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CrearNota(AudioManager.Current));
        }

        private void listaCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Notas nota = e.CurrentSelection.FirstOrDefault() as Notas;

            if (nota != null)
            {
                DisplayAlert("ID del Nota", $"ID: {nota.IdNota}", "OK");
            }
        }

        private async void Eliminar_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (listaCollection.SelectedItem is Notas nota)
                {
                    Console.WriteLine($"Eliminando nota con ID: {nota.IdNota}, Descripción: {nota.Descripcion}");

                    await client.Child("Notas").Child(nota.IdNota).DeleteAsync();

                    Lista.Remove(nota);

                    // Actualizar la propiedad ItemsSource de listaCollection después de cambios en Lista
                    listaCollection.ItemsSource = Lista.ToList();
                }
                else
                {
                    await DisplayAlert("Alerta", "Selecciona una nota para eliminar", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar la nota: {ex.Message}");
            }
        }

        private async void Actualizar_Clicked(object sender, EventArgs e)
        {
            Notas nota = listaCollection.SelectedItem as Notas;

            if (nota != null)
            {
                await Navigation.PushAsync(new ActualizarNota(nota));
            }
            else
            {
                await DisplayAlert("Alerta", "Selecciona un empleado para actualizar", "OK");
            }
        }

        private async void ReproducirAudio_Clicked(object sender, EventArgs e)
        {
            Notas nota = listaCollection.SelectedItem as Notas;

            if (nota != null && !string.IsNullOrEmpty(nota.AudioRecord))
            {
                MediaElement mediaElement = new MediaElement
                {
                    Source = nota.AudioRecord,
                    ShouldAutoPlay = true
                };
                container.Add(mediaElement);
            }
            else
            {
                await DisplayAlert("Alerta", "Selecciona una nota con audio para reproducir", "OK");
            }
        }
    }

}
