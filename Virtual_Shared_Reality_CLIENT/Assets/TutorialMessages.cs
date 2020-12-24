using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets
{
    class TutorialMessages
    {
        //COMMON MESSAGES
        public static string TryAgainText(float points, float Threshold)
        {
            points *= 100;
            string scoreS = String.Format("{0:0.0}", points);
          
            return " Sono sicuro che puoi fare di meglio, il tuo punteggio e' " + scoreS + "% ma per passare la prova ti serve " + Threshold * 100 + "%";
        }

        /// ANDROID MESSAGES:
        /// 
#if UNITY_ANDROID
        public static readonly string intro = "Per iniziare il Tutorial inquadra il target con la fotocamera del tuo tablet, dopodiché seleziona l'elemento virtuale con un dito e trascinalo nella sua copia trasparente." +
                                "Quando hai finito, tocca il bottone rosso";
        public static readonly string translationXText = "Per mutare modalità di spostamento, mostrata dall'icona in alto a sinistra, tocca due volte lo schermo. " +
                                "Cambia modalità per poter muovere l'elemento in profondità, cambia modalità nuovamente per spostarlo in alto e in basso. Sposta l'elemento nella sua copia trasparente, quando hai terminato clicca il bottone";
        public static readonly string twistText = "Per continuare con il tutorial seleziona l'oggetto e prova a rutorarlo intorno al suo asse principale, effettuando un twist con le dita come " +
                                "mostrato nell'animazione di lato";
         public static readonly string rotationYText = "Ben fatto! Ora riseleziona l'oggetto e prova a rutorarlo intorno al suo asse verticale, facendo scorrere due dita poste " +
                                "una vicino all'altra a destra per ruotarlo in senso orario e a sinistra per farlo in senso antiorario (Come mostrato dall'animazione qui a lato)";
        public static readonly string rotationZText = "Dopo aver selezionato un oggetto, prova a rutorarlo intorno al suo asse orizzontale, facendo scorrere due dita poste " +
                                "una vicino all'altra in alto per ruotarlo in senso orario e in basso per farlo in senso antiorario (Come mostrato dall'animazione qui a lato)";
        public static readonly string rotationTaskText = "Ora prova ad utilizzare le skills appena apprese per ruotare l'oggetto virtuale e a spostarlo in modo che combaci con la sua copia trasparente";

        public static readonly string scaleText = "Sposta l'oggetto nelle vicinanze della sua copia trasparente e prova ad ingrnadirlo/rimpicciolirlo, selezionandolo e effettuando " +
                        "uno zoom out per ingrandirlo e uno zoom out e subito dopo in per rimpicciolirlo, come mostrato dall'animazione qui a lato ";
        public static readonly string tutorialEnd = "Complimenti hai finito il tutorial, ora sei pronto per la vera sfida";
#else

        // HMD MESSAGES:
        public static readonly string moveTableText = "Benvenuto! Posiziona il controller  sopra il target che vedi al di fuori della realtà virtuale in direzione dell'azze Z e premi il 'Button1' ovvero 'A' se stai usando il controller destro" +
           " oppure 'X' se stai usando quello sinistro";
        public static readonly string laserManagementText = "Per attivare il laser tieni premuto il bottone Grip, dopodiché per selezionare una scelta da un menu, premi il bottone Trigger";
        public static readonly string intro = "Seleziona l'elemento virtuale spostando il controller vicino a questi e premendo il tasto 'Trigger', dopodiché trascinalo nella sua copia trasparente." +
                                "Quando hai finito, spingi con il controller il bottone rosso";
        public static readonly string scaleText = "Seleziona l'oggetto come spiegato precedentemente, dopodiché spingi in alto il 'ThumbStick' e cliccalo per ingrandirlo oppure fai lo stesso in basso per rimpicciolirlo." +
            " Dopodiché sposta l'oggetto nella sua copia trasparente, ingrandiscilo in modo che combaci con essa e quando hai finito, clicca il bottone";
        public static readonly string tutorialEnd = "Complimenti hai finito il tutorial, ora sei pronto per la vera sfida";
       
#endif
    }
}
