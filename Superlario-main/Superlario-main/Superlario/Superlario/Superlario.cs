using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Net.Sockets;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

//versio 1.0

namespace Superlario;

public class Superlario : PhysicsGame
{
    private const double NOPEUS = 200;
    private const double HYPPYNOPEUS = 750;
    private const int RUUDUN_KOKO = 50;

    private PlatformCharacter pelaaja1;

    private Image pelaajanKuva = LoadImage("D.png");
    private Image tahtiKuva = LoadImage("Coin.png");
    private Image kissaf = LoadImage("K.png");
    private Image bossi = LoadImage("norsu.png");

    private List<GameObject> liikutettavat = new List<GameObject>();
    private double SUUNTA = 1.0;

    public override void Begin()
    {
        Gravity = new Vector(0, -1000);

        LuoKentta();
        LisaaNappaimet();
        LuoPistelaskuri();

        Camera.Follow(pelaaja1);
        Camera.ZoomFactor = 1.4;
        Camera.StayInLevel = true;

        MasterVolume = 0.5;

    }

    private void LuoKentta()
    {
        TileMap kentta = TileMap.FromLevelAsset("kentta1.txt");
        kentta.SetTileMethod('#', LisaaTaso);
        kentta.SetTileMethod('*', LisaaTahti);
        kentta.SetTileMethod('N', LisaaPelaaja);
        kentta.SetTileMethod('M', Lisaakissa);
        kentta.SetTileMethod('L', Lisaalattia);
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);
        Level.CreateBorders();
        Level.Background.CreateGradient(Color.White, Color.SkyBlue);
        
        Timer liikutusajastin = new Timer();
        liikutusajastin.Interval = 0.02;
        liikutusajastin.Timeout += LiikutaOlioita;
        liikutusajastin.Start();
    }

    private void LiikutaOlioita()
    {
        for (int i = 0; i < liikutettavat.Count; i++)
        {
            GameObject olio = liikutettavat[i];
            olio.X += SUUNTA;

        }
    }

private void LisaaTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Color = Color.Brown;
        Add(taso);
    }

    private void Lisaalattia(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject lattia = PhysicsObject.CreateStaticObject(leveys, korkeus);
        lattia.Position = paikka;
        lattia.Color = Color.Brown;
        AddCollisionHandler(lattia, "kissa", TormaaLattiaan);
        Add(lattia);
    }

    private void LisaaTahti(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject tahti = PhysicsObject.CreateStaticObject(leveys, korkeus);
        tahti.IgnoresCollisionResponse = true;
        tahti.Position = paikka;
        tahti.Image = tahtiKuva;
        tahti.Tag = "tahti";
        Add(tahti);
    }

    private void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        pelaaja1 = new PlatformCharacter(39, 39);
        pelaaja1.Position = paikka;
        pelaaja1.Mass = 3.0;
        pelaaja1.Image = pelaajanKuva;
        AddCollisionHandler(pelaaja1, "tahti", TormaaTahteen);
        AddCollisionHandler(pelaaja1, "kissa", Tormaakissaan);
        Add(pelaaja1);
    }

    private void Lisaakissa(Vector paikka, double leveys, double korkeus)
    {

        PhysicsObject kissa = new PhysicsObject(40, 40);
        kissa.Image = kissaf;
        kissa.Restitution = 1.0;
        kissa.Position = paikka;
        AddCollisionHandler(kissa, "Lattia", TormaaLattiaan);
        kissa.Tag = "kissa";
        Add(kissa);
        liikutettavat.Add(kissa);
    }


private void LisaaNappaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

        Keyboard.Listen(Key.A, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, -NOPEUS);
        Keyboard.Listen(Key.D, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, NOPEUS);
        Keyboard.Listen(Key.W, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, HYPPYNOPEUS);

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
    }

    private void Liikuta(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Walk(nopeus);
    }


    private void Hyppaa(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Jump(nopeus);
    }

    private void TormaaTahteen(PhysicsObject hahmo, PhysicsObject tahti)
    {
        MessageDisplay.Add("Coin collected!");
        tahti.Destroy();
        pistelaskuri.Value += 5;
    }

    private void Tormaakissaan(PhysicsObject hahmo, PhysicsObject kissa)
    {
        pelaaja1.Destroy();
        ConfirmExit();
    }
    
    private void TormaaLattiaan(PhysicsObject lattia, PhysicsObject kissa)
    {
        SUUNTA = SUUNTA * -1;
    }


    IntMeter pistelaskuri;

    void LuoPistelaskuri()
    {
        pistelaskuri = new IntMeter(0);

        Label pistenaytto = new Label();
        pistenaytto.X = Screen.Left + 1300;
        pistenaytto.Y = Screen.Top - 10;
        pistenaytto.TextColor = Color.Black;
        pistenaytto.Color = Color.White;

        pistenaytto.BindTo(pistelaskuri);
        Add(pistenaytto);
        pistenaytto.Title = "Points:";
    }
}