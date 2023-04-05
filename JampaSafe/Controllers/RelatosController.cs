using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Dados;
using System.Net.Mail;
using JampaSafe.Models;


namespace JampaSafe.Controllers
{
    public class RelatosController : Controller
    {
        private DadosEntities db = new DadosEntities();

        public ActionResult Index() {
            return View(db.Relato.ToList());
        }

        public ActionResult Create() {
            ViewBag.codtipo = new SelectList(db.TipodeRelato,"codtipo","nome");
            return PartialView("_Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idrelato,data,codtipo,descricao,bairro")] Relato relato, [Bind(Include = "imagem")] HttpPostedFileBase imagem)
        {
            if (imagem != null && imagem.ContentLength > 0) //VERIFICA SE O ARQUIVO FILE RECEBIDO NÃO É NULO E TB A SUA QTD DE BYTES
            {
                relato.imagem3= new byte[imagem.ContentLength];
                using (var reader = new BinaryReader(imagem.InputStream))
                {
                    reader.Read(relato.imagem3, 0, imagem.ContentLength);

                }
                if (ModelState.IsValid)
                {
                    relato.bairro = relato.bairro.ToUpper();
                    relato.data = relato.data.Date;
                    db.Relato.Add(relato);
                    db.SaveChanges();
                    EnviaEmail(Json(relato));
                    return RedirectToAction("Index");
                }
            }


                ViewBag.codtipo = new SelectList(db.TipodeRelato, "codtipo", "nome");
                return Content("Falha no envio do formulário!");

            }
        

        public FileResult RetornaUrl(int id)
        {
            var model = db.Relato.Find(id);
            return File(model.imagem3, "image/jpeg"); //qual url isso vai gerar no src??
        }



        public ActionResult Delete(int? id) {


            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (db.Relato.Find(id)==null) {
                return HttpNotFound();
            }
            var relato = db.Relato.Find(id);
            db.Relato.Remove(relato);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public ActionResult Update(int? id)   //o ? indica que o parametro é opcional, ou seja, ele pode atribuir um valor Nulo
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (db.Relato.Find(id) == null)
            {
                return HttpNotFound();
            }
            ViewBag.codtipo = new SelectList(db.TipodeRelato, "codtipo", "nome");
            var relato = db.Relato.Find(id);
            return PartialView("_Edit",relato);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idrelato,data,codtipo,descricao,bairro")] Relato relato,[Bind(Include ="imagem")] HttpPostedFileBase imagem) {

            if (imagem != null && imagem.ContentLength > 0) //VERIFICA SE O ARQUIVO FILE RECEBIDO NÃO É NULO E TB A SUA QTD DE BYTES
            {
                relato.imagem3 = new byte[imagem.ContentLength];
                using (var reader = new BinaryReader(imagem.InputStream))
                {
                    reader.Read(relato.imagem3, 0, imagem.ContentLength);

                }
            }

                if (ModelState.IsValid)
            {
                relato.bairro = relato.bairro.ToUpper();
                relato.data = relato.data.Date;
                db.Entry(relato).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }


            ViewBag.codtipo = new SelectList(db.TipodeRelato, "codtipo", "nome");
            return Content("ERRO NA ATUALIZAÇÃO DO RELATO");
        }


        public ActionResult Search(string tipo) {

            var model = db.Relato.Where(m=>m.TipodeRelato.nome.Contains(tipo));
            return PartialView("_Query", model);
        
        }

        public ActionResult Ranking()
        {
            var model = db.Relato.Select(m => m.bairro);
            return PartialView("_Ranking", model);
        }


        public ActionResult Ocorrencias(string  bairro ) {

            var bairros = db.Relato.Select(m => m.bairro);

            int count = bairros.Where(m => m.Equals(bairro)).Count();

            return Content(count.ToString());
        }


        public ActionResult FormatarData(DateTime data) {

            return Content(data.ToString("dd/MM/yyyy"));
        }


        public ActionResult Cont(string tipo)
        {
            var model = db.Relato.Where(m => m.TipodeRelato.nome.Contains(tipo));
            int cont = model.Count();
            return Content(cont.ToString());
        }


        public void EnviaEmail(JsonResult json) {

            ////Dados do EMAIL
            string remetente = "joaomarcos77777.jm@gmail.com";
            string destinatario = "joaomarcos77777.jm@gmail.com";
            string assunto = "NOVO RELATO JAMPASAFE";
            string corpo = json.Data.ToString();

            try
            {
                MailMessage msg = new MailMessage(remetente, destinatario, assunto, corpo); //CRIACAO DO OBJETO EMAIL MSG
                SmtpClient cliente = new SmtpClient("smtp.gmail.com",587); //CRIACAO DO OBJETO SERVIDOR E A PORTA SMTP PROTOCOLO
                cliente.EnableSsl = true;
                cliente.Credentials = new NetworkCredential("joaomarcos77777.jm@gmail.com", "xxxxxxxxx"); //CREDENCIAIS DO REMETENTE
                cliente.Send(msg); //ENVIO DA MSG AO SMPTCLIENTE 
            }
            catch(Exception e)
            {
               Console.WriteLine(e.ToString());
            }
        }


     

        protected override void Dispose(bool disposing) //POLIFORMISMO DO METODO DISPOSE DA CLASSE CONTROLLER HERDADA
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing); //BASE SE REFERE AO QUE ACONTECE NA CLASSE HERDADA!
        }
    }
}
