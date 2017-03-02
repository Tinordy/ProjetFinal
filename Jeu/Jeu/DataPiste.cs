using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AtelierXNA
{
    class DataPiste
    {
        const int NB_COEFFICIENTS_PAR_LIGNE = 4;
        const int NB_PTS_INTERMÉDIAIRES = 34;
        const int INTERVALLE_POINTS_PATROUILLE = 5;
        const float LARGEUR_PISTE = 10;
        const string CHEMIN = "../../../";
        const char ESPACE = ' ';
        const char TAB = '\t';
        const float ÉCHELLE = 2;


        List<float[]> CoefficientsX { get; set; }
        List<float[]> CoefficientsY { get; set; }
        List<Vector2> PointsCube { get; set; }
        List<Vector2> PointsBordureExt { get; set; }
        List<Vector2> PointsBordureInt { get; set; }
        List<Vector2> PointsCentraux { get; set; }
        List<Vector2> PointsDePatrouille { get; set; }
        List<Vector2> PointsPointillés { get; set; }
        float IncrémentX { get; set; }
        string SplineX { get; set; }
        string SplineY { get; set; }
        float LargeurPointillée { get; set; }
        public Vector2 PositionAvatar
        {
            get
            {
                return PointsCube[0];
            }
        }

        public DataPiste(string splineX, string splineY)
        {
            SplineX = splineX;
            SplineY = splineY;
            CoefficientsX = AssignationDesDonnées(SplineX);
            CoefficientsY = AssignationDesDonnées(SplineY);

            CalculerPointsCube();
            CalculerPointsCentraux();
            CalculerBordures();
            CalculerPointsDePatrouille();
        }

        static List<float[]> AssignationDesDonnées(string nomFichier)
        {
            char[] séparateurs = new char[] { ESPACE, TAB };
            string[] coefficientsString;
            List<float[]> coefficientsFloat = new List<float[]>();
            StreamReader fichierSpline = new StreamReader(CHEMIN + nomFichier);
            int noÉquation = 0;
            while (!fichierSpline.EndOfStream)
            {
                coefficientsString = fichierSpline.ReadLine().Split(séparateurs);
                coefficientsFloat.Add(new float[NB_COEFFICIENTS_PAR_LIGNE]);
                for (int i = 0; i < NB_COEFFICIENTS_PAR_LIGNE; ++i)
                {
                    coefficientsFloat[noÉquation][i] = float.Parse(coefficientsString[i]);
                }
                ++noÉquation;
            }
            return coefficientsFloat;
        }
        void CalculerPointsCube()
        {
            PointsCube = new List<Vector2>();
            for (int i = 0; i < CoefficientsX.Count; ++i)
            {
                PointsCube.Add(ÉCHELLE * new Vector2(Spline(CoefficientsX[i], i), Spline(CoefficientsY[i], i)));
            }
        }
        void CalculerPointsCentraux()
        {
            PointsCentraux = new List<Vector2>();
            for (int i = 0; i < CoefficientsX.Count; ++i)
            {
                for (int cpt = 0; cpt < NB_PTS_INTERMÉDIAIRES; ++cpt)
                {
                    PointsCentraux.Add(ÉCHELLE * new Vector2(Spline(CoefficientsX[i], i + (float)cpt / NB_PTS_INTERMÉDIAIRES), Spline(CoefficientsY[i], i + (float)cpt / NB_PTS_INTERMÉDIAIRES)));
                }
            }

            
        }
        void CalculerPointsPointillés()
        {
            PointsPointillés = new List<Vector2>();
            LargeurPointillée = LARGEUR_PISTE / 5f;

            for(int i = 0; i < PointsCentraux.Count; ++i)
            {
                Vector2 vecteurTemp = PointsBordureExt[i] - PointsCentraux[i];
                Vector2 vecteurNormalized = Vector2.Normalize(vecteurTemp);
                PointsPointillés.Add(PointsCentraux[i] + LargeurPointillée * (vecteurNormalized));
                PointsPointillés.Add(PointsCentraux[i] - LargeurPointillée * (vecteurNormalized));
            }
        }
        float Spline(float[] tab, float t)
        {
            return tab[0] + tab[1] * (float)Math.Pow(t, 3) + tab[2] * (float)Math.Pow(t, 2) + tab[3] * t;
        }
        void CalculerPointsDePatrouille()
        {

            PointsDePatrouille = new List<Vector2>();
            for (int cpt = 0; cpt < CoefficientsX.Count; ++cpt)
            {
                for (int cpt2 = 0; cpt2 < NB_PTS_INTERMÉDIAIRES / INTERVALLE_POINTS_PATROUILLE; ++cpt2)
                {
                    PointsDePatrouille.Add(PointsCentraux[INTERVALLE_POINTS_PATROUILLE * cpt2 + NB_PTS_INTERMÉDIAIRES * cpt]);
                }
            }
        }
        void CalculerBordures()
        {
            PointsBordureExt = new List<Vector2>();
            PointsBordureInt = new List<Vector2>();

            for (int cpt = 0; cpt < NB_PTS_INTERMÉDIAIRES * CoefficientsX.Count; ++cpt)
            {
                Vector2 vDirecteur = Vector2.Normalize(PointsCentraux[cpt] - PointsCentraux[(cpt + 1) % (NB_PTS_INTERMÉDIAIRES * CoefficientsX.Count)]) * LARGEUR_PISTE;
                PointsBordureExt.Add(PointsCentraux[cpt] + new Vector2(-vDirecteur.Y, vDirecteur.X));
                PointsBordureInt.Add(PointsCentraux[cpt] + new Vector2(vDirecteur.Y, -vDirecteur.X));
            }
            CalculerPointsPointillés();
        }

        public List<Vector2> GetPointsCentraux()
        {
            return CopierListeVecteur2(PointsCentraux);
        }
        public List<Vector2> GetPointsPointillés()
        {
            return CopierListeVecteur2(PointsPointillés);
        }

        public List<Vector2> GetBordureExtérieure()
        {
            return CopierListeVecteur2(PointsBordureExt);
        }
        public List<Vector2> GetBordureIntérieur()
        {
            return CopierListeVecteur2(PointsBordureInt);
        }
        public List<Vector2> GetPointsCube()
        {
            return CopierListeVecteur2(PointsCube);
        }
        public List<Vector2> GetPointsDePatrouille()
        {
            return CopierListeVecteur2(PointsDePatrouille);
        }
        List<Vector2> CopierListeVecteur2(List<Vector2> listeÀCopier)
        {
            List<Vector2> copieEnProfondeur = new List<Vector2>();
            foreach (Vector2 vÀCopier in listeÀCopier)
            {
                copieEnProfondeur.Add(new Vector2(vÀCopier.X, vÀCopier.Y));
            }
            return copieEnProfondeur;
        }
    }
}
