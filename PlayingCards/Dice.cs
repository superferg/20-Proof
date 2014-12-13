using System;
using System.ComponentModel;
using System.Threading;
using System.Drawing;
using System.Resources;
using System.Media;
using System.IO;

namespace PlayingCards
{
	public class Dice
	{
	    private Random _Random;
	    private BackgroundWorker _Worker;
	    
	    private static System.Resources.ResourceManager  resourceManager =
            new System.Resources.ResourceManager("PlayingCards.Images", System.Reflection.Assembly.GetExecutingAssembly());
	
	    /// <summary>
	    /// Initializes a new instance of the <see cref="Dice"/> class.
	    /// </summary>
	    public Dice()
	    {
	        _Random = new Random();
	
	        InitializeDefaultValues();
	        InitializeBackgroundWorker();
	    }
	
	    /// <summary>
	    /// Occurs when the dice finished rolling.
	    /// </summary>
	    public event EventHandler Rolled;
	
	    /// <summary>
	    /// Occurs while the dice is rolling and the value has changed.
	    /// </summary>
	    public event EventHandler RollingChanged;
	
	    /// <summary>
	    /// Gets or sets the including maximum value that the dice can return.
	    /// </summary>
	    /// <value>
	    /// The maximum value.
	    /// </value>
	    [DefaultValue(6)]
	    public int Maximum { get; set; }
	
	    /// <summary>
	    /// Gets or sets the including minimum value that the dice can return.
	    /// </summary>
	    /// <value>
	    /// The minimum.
	    /// </value>
	    [DefaultValue(1)]
	    public int Minimum { get; set; }
	
	    /// <summary>
	    /// Gets the result that this dice currently has.
	    /// </summary>
	    public int Result { get; private set; }
	
	    /// <summary>
	    /// Gets or sets the duration of the rolling.
	    /// </summary>
	    /// <value>
	    /// The duration of the rolling.
	    /// </value>
	    [DefaultValue(typeof(TimeSpan), "00:00:00.6")]
	    public TimeSpan RollingDuration { get; set; }
	
	    /// <summary>
	    /// Starts rolling the dice.
	    /// </summary>
	    public void Roll()
	    {
	        if (!_Worker.IsBusy)
	        {
	            CheckParameters();
	            _Worker.RunWorkerAsync();
	        }
	    }
	
	    private void CheckParameters()
	    {
	        if (Minimum >= Maximum)
	        {
	            throw new InvalidOperationException("Minimum value must be less than the Maximum value.");
	        }
	
	        if (RollingDuration <= TimeSpan.Zero)
	        {
	            throw new InvalidOperationException("The RollingDuration must be greater zero.");
	        }
	    }
	
	    private void InitializeBackgroundWorker()
	    {
	        _Worker = new BackgroundWorker();
	        _Worker.WorkerReportsProgress = true;
	        _Worker.DoWork += OnWorkerDoWork;
	        _Worker.ProgressChanged += OnWorkerProgressChanged;
	        _Worker.RunWorkerCompleted += OnWorkerRunWorkerCompleted;
	    }
	
	    private void InitializeDefaultValues()
	    {
	        Minimum = 1;
	        Maximum = 6;
	        Result = Minimum;
	        RollingDuration = TimeSpan.FromMilliseconds(600);
	    }
	
	    private void OnWorkerDoWork(object sender, DoWorkEventArgs e)
	    {
	        var finishTime = DateTime.UtcNow + RollingDuration;
	
	        while (finishTime > DateTime.UtcNow)
	        {
	            Result = _Random.Next(Minimum, Maximum + 1);
	            _Worker.ReportProgress(0);
	            // ToDo: Improve sleep times for more realistic rolling.
	            Thread.Sleep(100);
	        }
	    }
	
	    private void OnWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
	    {
	        RaiseEvent(RollingChanged);
	    }
	
	    private void OnWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	    {
	        RaiseEvent(Rolled);
	    }
	
	    private void RaiseEvent(EventHandler handler)
	    {
	        var temp = handler;
	
	        if (temp != null)
	        {
	            temp(this, EventArgs.Empty);
	        }
	    }
	    
	    public static Bitmap GetDiceImage(int DiceValue)
        {
	    	Bitmap diceFace = null;
	    	
	    	if(DiceValue == 1) diceFace = (Bitmap)resourceManager.GetObject("blue-number-one-md");
	    	else if(DiceValue == 2) diceFace = (Bitmap)resourceManager.GetObject("blue-number-two-md");
	    	else if(DiceValue == 3) diceFace = (Bitmap)resourceManager.GetObject("blue-number-three-md");
	    	else if(DiceValue == 4) diceFace = (Bitmap)resourceManager.GetObject("blue-number-four-md");
	    	else if(DiceValue == 5) diceFace = (Bitmap)resourceManager.GetObject("blue-number-five-md");
	    	else if(DiceValue == 6) diceFace = (Bitmap)resourceManager.GetObject("blue-number-six-md");
	    	else if(DiceValue == 7) diceFace = (Bitmap)resourceManager.GetObject("blue-number-seven-md");
	    	else if(DiceValue == 8) diceFace = (Bitmap)resourceManager.GetObject("blue-number-eight-md");
	    	else if(DiceValue == 9) diceFace = (Bitmap)resourceManager.GetObject("blue-number-nine-md");
	    	else if(DiceValue == 10) diceFace = (Bitmap)resourceManager.GetObject("blue-number-zero-md");

            return diceFace;
        }
	    
	    public static void PlayDiceRollSound()
        {
	    	if(File.Exists("Sounds\\DiceRoll.wav"))
	    	{
	    		SoundPlayer player = new SoundPlayer("Sounds\\DiceRoll.wav");
		    	player.Play();
	    	}
        }
	}
}
