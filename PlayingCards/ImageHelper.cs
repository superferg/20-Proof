using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Resources;

namespace PlayingCards
{
    public static class ImageHelper
    {
        private static readonly System.Resources.ResourceManager  resourceManager =
            new System.Resources.ResourceManager("PlayingCards.Images", System.Reflection.Assembly.GetExecutingAssembly());
        
        private const int bytesPerPixel = 4;

        public static Bitmap GetFaceImageForCard(Card card)
        {
            Bitmap cardImages = (Bitmap)resourceManager.GetObject("cardfaces");
            
            int topx = 0;
            int topy = 0;

            if (card.Suit == CardSuits.Clubs) topy = 0;
            if (card.Suit == CardSuits.Spades) topy = 98;
            if (card.Suit == CardSuits.Hearts) topy = 196;
            if (card.Suit == CardSuits.Diamonds) topy = 294;

            topx = 73 * Convert.ToInt32(card.Value);

            Rectangle rect = new Rectangle(topx, topy, 73, 97);
            Bitmap cropped = cardImages.Clone(rect, cardImages.PixelFormat);

            return cropped;
        }

        public static Bitmap GetBackImage()
        {
            Bitmap cardBack = (Bitmap)resourceManager.GetObject("cardback");

            return cardBack;
        }
        
        public static Bitmap GetDeckStackImage()
        {
            Bitmap cardBack = (Bitmap)resourceManager.GetObject("card-back_deck");

            return cardBack;
        }
        
        public static Image ChangeImageOpacity(Image originalImage, double opacity)
    	{   
            
        	if ((originalImage.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
        	{
        		// Cannot modify an image with indexed colors
        		return originalImage;
        	}

        	Bitmap bmp = (Bitmap)originalImage.Clone();

        	// Specify a pixel format.
        	PixelFormat pxf = PixelFormat.Format32bppArgb;

        	// Lock the bitmap's bits.
        	Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        	BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);

        	// Get the address of the first line.
        	IntPtr ptr = bmpData.Scan0;

        	// Declare an array to hold the bytes of the bitmap.
        	// This code is specific to a bitmap with 32 bits per pixels
        	// (32 bits = 4 bytes, 3 for RGB and 1 byte for alpha).
        	int numBytes = bmp.Width * bmp.Height * bytesPerPixel;
        	byte[] argbValues = new byte[numBytes];

        	// Copy the ARGB values into the array.
        	System.Runtime.InteropServices.Marshal.Copy(ptr, argbValues, 0, numBytes);

        	// Manipulate the bitmap, such as changing the
        	// RGB values for all pixels in the the bitmap.
        	for (int counter = 0; counter < argbValues.Length; counter += bytesPerPixel)
        	{
        		// argbValues is in format BGRA (Blue, Green, Red, Alpha)

        		// If 100% transparent, skip pixel
        		if (argbValues[counter + bytesPerPixel - 1] == 0)
        			continue;

        		int pos = 0;
        		pos++; // B value
        		pos++; // G value
        		pos++; // R value

        		argbValues[counter + pos] = (byte) (argbValues[counter + pos] * opacity);
        	}

        	// Copy the ARGB values back to the bitmap
        	System.Runtime.InteropServices.Marshal.Copy(argbValues, 0, ptr, numBytes);

        	// Unlock the bits.
        	bmp.UnlockBits(bmpData);

        	return bmp;
        }
        
        public static Image SetBrightness(Image _currentBitmap, int brightness)
        {
        	Bitmap bmap = (Bitmap)_currentBitmap.Clone();
        	
        	if (brightness < -255) brightness = -255;
        	if (brightness > 255) brightness = 255;
        	Color c;
        	for (int i = 0; i < bmap.Width; i++)
        	{
        		for (int j = 0; j < bmap.Height; j++)
        		{
        			c = bmap.GetPixel(i, j);
        			int cR = c.R + brightness;
        			int cG = c.G + brightness;
        			int cB = c.B + brightness;

        			if (cR < 0) cR = 1;
        			if (cR > 255) cR = 255;

        			if (cG < 0) cG = 1;
        			if (cG > 255) cG = 255;

        			if (cB < 0) cB = 1;
        			if (cB > 255) cB = 255;

        			bmap.SetPixel(i, j, Color.FromArgb((byte)cR, (byte)cG, (byte)cB));
        		}
        	}
        	
        	return (Bitmap)bmap.Clone();
        }
        
        public static Bitmap GetSuitImage(CardSuits suit)
        {
        	Bitmap pic = null;
        	
            if (suit == CardSuits.Clubs) pic = (Bitmap)resourceManager.GetObject("Club-Card-Symbol");
            if (suit == CardSuits.Spades) pic = (Bitmap)resourceManager.GetObject("Spade-Card-Symbol");
            if (suit == CardSuits.Hearts) pic = (Bitmap)resourceManager.GetObject("Heart-Card-Symbol");
            if (suit == CardSuits.Diamonds) pic = (Bitmap)resourceManager.GetObject("Diamond-Card-Symbol");
            
            return pic;
        }
    }
}
