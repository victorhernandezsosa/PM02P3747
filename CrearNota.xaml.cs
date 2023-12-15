using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using Plugin.Maui.Audio;
using PM02P3747.Models;

namespace PM02P3747;

public partial class CrearNota : ContentPage
{
    FirebaseClient client = new FirebaseClient("https://examenpm02-default-rtdb.firebaseio.com/");
    private string PhotoRecord { get; set; }


    string lblaudio, lblfoto;
    private Notas note;

    string token = string.Empty;
    string rutastorage = "examenpm02.appspot.com";
    private readonly IAudioRecorder _audioRecorder;
    private bool isRecording = false;
    public string pathaudio, filename;


    public CrearNota(IAudioManager audioManager)
	{
		InitializeComponent();
        _audioRecorder = AudioManager.Current.CreateRecorder();
        BindingContext = this;
        note = new Notas();
        Generarid();
    }

    private void Generarid()
    {
        Random random = new Random();
        int numeroAleatorio = random.Next(1, 100);

        entryNumero.Text = numeroAleatorio.ToString();
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
                PhotoRecord = await new FirebaseStorage("examenpm02.appspot.com")
                    .Child("Fotos")
                    .Child(DateTime.Now.ToString("ddMMyyyyhhmmss") + ".jpg")
                    .PutAsync(stream);

                fotoImage.Source = PhotoRecord;
            }
            else
            {
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
            audio.Text = "Grabando";
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
                    lblaudio = urlDescarga;
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
            audio.Text = "Grabar Audio";
            Console.WriteLine("Deteniendo grabación y guardando el audio...");
        }

    }



    private async void save_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(Descripciontxt.Text) || FechaPicker.Date < DateTime.Now || string.IsNullOrEmpty(audio.Text) || fotoImage.Source == null)
        {
            await DisplayAlert("Nota no Realizada", "Rellene todo los campos", "OK");
        }
        else
        {
            int currentCounter = await note.GetCounterAsync();
            int newId = currentCounter + 1;

            await client.Child("Notas").Child(newId.ToString()).PutAsync(new Notas
            {
                IdNota = newId.ToString(),
                Descripcion = Descripciontxt.Text,
                Fecha = FechaPicker.Date, 
                PhotoRecord = PhotoRecord,
                AudioRecord = lblaudio
            });
            await note.UpdateCounterAsync(newId);
            await Shell.Current.GoToAsync("..");
            await DisplayAlert("Nota añadida Correctamente", "Correctamente", "OK");
        }
    }
}