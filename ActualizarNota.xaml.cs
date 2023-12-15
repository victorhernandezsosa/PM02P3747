using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using Plugin.Maui.Audio;
using PM02P3747.Models;
using System;


namespace PM02P3747;

public partial class ActualizarNota : ContentPage
{
    private FirebaseClient client = new FirebaseClient("https://pm02e3-default-rtdb.firebaseio.com/");
    private string PhotoRecord { get; set; }
    private Notas NotasToUpdate;

    private string token = string.Empty;
    private string rutastorage = "examenpm02.appspot.com";
    private string lblaudios, lblfoto;

    private readonly IAudioRecorder _audioRecorder;
    private bool isRecording = false;
    public string pathaudio, filename;
    public ActualizarNota(Notas NotasToUpdate)
    {
        InitializeComponent();
        _audioRecorder = new AudioManager().CreateRecorder();

        BindingContext = this;
        this.NotasToUpdate = NotasToUpdate;

        Descripciontxt.Text = NotasToUpdate.Descripcion;
        FechaPicker.Date = NotasToUpdate.Fecha;
        lblaudios = NotasToUpdate.AudioRecord;
        fotoImage.Source = NotasToUpdate.PhotoRecord;
    }

    private async void foto_Clicked(object sender, EventArgs e)
    {
        try
        {
            var photoOptions = new MediaPickerOptions
            {
                Title = "Tomar foto"
            };

            var foto = await MediaPicker.CapturePhotoAsync(photoOptions);

            if (foto != null)
            {
                var stream = await foto.OpenReadAsync();
                PhotoRecord = await new FirebaseStorage(rutastorage)
                    .Child("Fotos")
                    .Child(DateTime.Now.ToString("ddMMyyyyhhmmss") + ".jpg")
                    .PutAsync(stream);

                // Actualiza la propiedad PhotoRecord
                lblfoto = PhotoRecord;
                // Actualiza la imagen en la interfaz de usuario
                fotoImage.Source = lblfoto;
            }
            else
            {
                // Puedes mostrar un mensaje aquí si es necesario
                Console.WriteLine("Se canceló la captura de la foto");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al tomar la foto: {ex.Message}");
        }
    }

    private async void audio_Clicked(object sender, EventArgs e)
    {
        if (!isRecording)
        {
            var permiso = await Permissions.RequestAsync<Permissions.Microphone>();
            var permiso1 = await Permissions.RequestAsync<Permissions.StorageRead>();
            var permiso2 = await Permissions.RequestAsync<Permissions.StorageWrite>();

            if (permiso != PermissionStatus.Granted || permiso1 != PermissionStatus.Granted || permiso2 != PermissionStatus.Granted)
            {
                return;
            }
            await _audioRecorder.StartAsync();
            isRecording = true;
            audios.Text = "Grabando";
            Console.WriteLine("Iniciando grabación...");
        }
        else
        {
            var recordedAudio = await _audioRecorder.StopAsync();

            if (recordedAudio != null)
            {
                try
                {
                    filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DateTime.Now.ToString("ddMMyyyymmss") + "_VoiceNote.wav");

                    using (var fileStorage = new FileStream(filename, FileMode.Create, FileAccess.Write))
                    {
                        recordedAudio.GetAudioStream().CopyTo(fileStorage);
                    }

                    pathaudio = filename;

                    var task = new FirebaseStorage(
                        rutastorage,
                        new FirebaseStorageOptions
                        {
                            AuthTokenAsyncFactory = () => Task.FromResult(token),
                            ThrowOnCancel = true
                        }
                    )
                    .Child("Audios")
                    .Child(Path.GetFileName(pathaudio))
                    .PutAsync(File.OpenRead(pathaudio));

                    var urlDescarga = await task;
                    lblaudios = urlDescarga;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    await DisplayAlert("Error", "Ocurrió un error al procesar la grabación.", "Ok");
                }
            }
            else
            {
                await DisplayAlert("Error", "La grabación de audio ha fallado.", "Ok");
            }
            isRecording = false;
            audios.Text = "Grabar Audio";
            Console.WriteLine("Deteniendo grabación y guardando el audio...");
        }



    }

    private async void save_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(Descripciontxt.Text) || FechaPicker.Date < DateTime.Now || string.IsNullOrEmpty(lblaudios) || string.IsNullOrEmpty(lblfoto))
        {
            await DisplayAlert("Nota no Realizada", "Rellene todo los campos", "OK");
        }
        else
        {
            try
            {
                var updatedNota = new Notas
                {
                    IdNota = NotasToUpdate.IdNota,
                    Descripcion = Descripciontxt.Text,
                    Fecha = FechaPicker.Date,
                    PhotoRecord = lblfoto,
                    AudioRecord = lblaudios
                };

                await client.Child("Notas").Child(updatedNota.IdNota).PutAsync(updatedNota);

                Console.WriteLine($"URL de la imagen: {lblfoto}");

                await Shell.Current.GoToAsync("..");
                await DisplayAlert("Nota Actualizada", "Correctamente", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar la nota: {ex.Message}");
                await DisplayAlert("Error", $"Error al actualizar la nota: {ex.Message}", "OK");
            }
        }
    }

}