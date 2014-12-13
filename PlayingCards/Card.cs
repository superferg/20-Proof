using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Resources;

namespace PlayingCards
{
    public enum CardSuits
    {
        Clubs,
        Diamonds,
        Hearts,
        Spades
    }
    
    public enum CardColor
    {
        Red,
        Black
    }

    public enum CardValues
    {
        Deuce, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace
    }

    public enum FacingSides
    {
        FaceUp,
        FaceDown
    }

    public class Card: IComparable<Card>
    {
        private Image faceImage;
        private Image backImage;
        private Image rotatedfaceImage;
        private Image rotatedbackImage;
        private FacingSides facingSide = FacingSides.FaceDown;

        public Card(CardSuits suit, CardValues cardVal)
        {
            Suit = suit;
            Value = cardVal;
            
            if(suit == CardSuits.Clubs || suit == CardSuits.Spades)
            {
            	Color = CardColor.Black;
            }
            else
            {
            	Color = CardColor.Red;
            }
            
            faceImage = ImageHelper.GetFaceImageForCard(this);
            backImage = ImageHelper.GetBackImage();
            
            Bitmap bitmap1 = ImageHelper.GetFaceImageForCard(this);
            bitmap1.RotateFlip(RotateFlipType.Rotate90FlipNone);
            rotatedfaceImage = bitmap1;
            
            bitmap1 = ImageHelper.GetBackImage();
            bitmap1.RotateFlip(RotateFlipType.Rotate90FlipNone);
            rotatedbackImage = bitmap1;
            
            if(this.Value >= CardValues.Nine)
            {
            	faceImage = ImageHelper.ChangeImageOpacity(faceImage, 0.82);
            	backImage = ImageHelper.ChangeImageOpacity(backImage, 0.82);
            	rotatedfaceImage = ImageHelper.ChangeImageOpacity(rotatedfaceImage, 0.82);
            	rotatedbackImage = ImageHelper.ChangeImageOpacity(rotatedbackImage, 0.82);
            }
        }

        public CardSuits Suit{ get; set; }
		public CardValues Value{ get; set; }
        public CardColor Color{ get; set; }
		public string Tag { get;set; }

        public Image FaceImage
        {
            get { return faceImage; }
        }

        public Image BackImage
        {
            get { return backImage; }
        }
        
        public Image RotatedFaceImage
        {
            get { return rotatedfaceImage; }
        }

        public Image RotatedBackImage
        {
            get { return rotatedbackImage; }
        }

        public FacingSides FacingSide
        {
            get { return facingSide; }
            set { facingSide = value; }
        }

        public Image FacingImage
        {
            get {
                if (facingSide == FacingSides.FaceDown)
                    return BackImage;
                else
                    return FaceImage;
            }
        }
        
        public Image RotatedFacingImage
        {
            get {
                if (facingSide == FacingSides.FaceDown)
                    return RotatedBackImage;
                else
                    return RotatedFaceImage;
            }
        }

        public string TextValue
        {
            get
            {
                string val = "";

                val += Convert.ToInt32(this.Value + 2).ToString();

                if (val == "14") val = "A";
                if (val == "11") val = "J";
                if (val == "12") val = "Q";
                if (val == "13") val = "K";

                if (this.Suit == CardSuits.Clubs) val += ",Clubs";
                if (this.Suit == CardSuits.Diamonds) val += ",Diamonds";
                if (this.Suit == CardSuits.Hearts) val += ",Hearts";
                if (this.Suit == CardSuits.Spades) val += ",Spades";

                return val;
            }
        }

        public FacingSides Flip()
        {
            if (facingSide == FacingSides.FaceDown)
                this.FacingSide = FacingSides.FaceUp;
            else
                this.FacingSide = FacingSides.FaceDown;

            return FacingSide;
        }

        public override string ToString()
        {
            string val = "";

            val += Convert.ToInt32(this.Value + 2).ToString();

            if (val == "14") val = "A";
            if (val == "11") val = "J";
            if (val == "12") val = "Q";
            if (val == "13") val = "K";

            if (this.Suit == CardSuits.Clubs) val += ",Clubs";
            if (this.Suit == CardSuits.Diamonds) val += ",Diamonds";
            if (this.Suit == CardSuits.Hearts) val += ",Hearts";
            if (this.Suit == CardSuits.Spades) val += ",Spades";

            return val;
        }

        public int CompareTo(Card c)
	    {
	          // A null value means that this object is greater. 
	        if (c == null)
	            return 1;
	
	        else 
	            return this.Value.CompareTo(c.Value);
	    }
    }
}
