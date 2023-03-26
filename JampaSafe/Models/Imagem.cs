using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JampaSafe.Models
{
    public class Imagem
    {

        public string tipoimagem { get; set; }
        public Imagem(string type) {
            tipoimagem = type;
        }
    }
}