using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;

namespace PM02P3747.Models
{
    public class Notas
    {

        FirebaseClient client = new FirebaseClient("https://examenpm02-default-rtdb.firebaseio.com/");
        public async Task<int> GetCounterAsync()
        {
            var counterSnapshot = await client.Child("contador").OnceSingleAsync<int?>();
            return counterSnapshot ?? 0;
        }

        public async Task UpdateCounterAsync(int newCounterValue)
        {
            await client.Child("contador").PutAsync(newCounterValue);
        }

        public string IdNota { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public string PhotoRecord { get; set; }
        public string AudioRecord { get; set; }
    }
}
