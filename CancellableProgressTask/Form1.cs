using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CancellableProgressTask
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource _source;

        public Form1()
        {
            InitializeComponent();
            _source = new CancellationTokenSource();
        }

        private void button1_Click(object sender, EventArgs eventArgs)
        {

            // Define the cancellation token.
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            Random rnd = new Random();
            Object lockObj = new Object();

            List<Task<int[]>> tasks = new List<Task<int[]>>();
            TaskFactory factory = new TaskFactory(token);
            for (int taskCtr = 0; taskCtr <= 10; taskCtr++)
            {
                int iteration = taskCtr + 1;
                tasks.Add(factory.StartNew(() => {
                    int value;
                    int[] values = new int[10];
                    for (int ctr = 1; ctr <= 10; ctr++)
                    {
                        lock (lockObj)
                        {
                            value = rnd.Next(0, 101);
                            Console.WriteLine("Task {0} Value {1}", iteration,value);
                        }
                        if (value == 0)
                        {
                            source.Cancel();
                            Console.WriteLine("Cancelling at task {0}", iteration);
                            break;
                        }
                        values[ctr - 1] = value;
                    }
                    return values;
                }, token));

            }
            try
            {
                Task<double> fTask = factory.ContinueWhenAll(tasks.ToArray(),
                                                             (results) => {
                                                                 Console.WriteLine("Calculating overall mean...");
                                                                 long sum = 0;
                                                                 int n = 0;
                                                                 foreach (var t in results)
                                                                 {
                                                                     foreach (var r in t.Result)
                                                                     {
                                                                         sum += r;
                                                                         n++;
                                                                     }
                                                                 }
                                                                 return sum / (double)n;
                                                             }, token);
                Console.WriteLine("The mean is {0}.", fTask.Result);
            }
            catch (AggregateException ae)
            {
                foreach (Exception e in ae.InnerExceptions)
                {
                    if (e is TaskCanceledException)
                        Console.WriteLine("Unable to compute mean: {0}",
                                          ((TaskCanceledException)e).Message);
                    else
                        Console.WriteLine("Exception: " + e.GetType().Name);
                }
            }
            finally
            {
                source.Dispose();
            }
        }

        private void button2_Click(object sender, EventArgs err)
        {
            var progressView = new ProgressView(_source);
            progressView.Show(this);
            //var task = new Task(()=>act(progressView));
            var tasks = new List<Task>();
            

            TaskFactory factory = new TaskFactory(_source.Token);
            tasks.Add(factory.StartNew(() => act(progressView)));
            //var startNew = factory.StartNew(() => act(progressView));

            try
            {
                factory.ContinueWhenAll(tasks.ToArray(), (r) => { progressView.Close(); }, _source.Token);
            }
            catch (AggregateException ae)
            {
                foreach (Exception e in ae.InnerExceptions)
                {
                    if (e is TaskCanceledException)
                        Console.WriteLine("Unable to compute mean: {0}",
                            ((TaskCanceledException)e).Message);
                    else
                        Console.WriteLine("Exception: " + e.GetType().Name);
                }
            }
            catch (Exception exception)
            {

            }

            

        }

        private void act(ProgressView progressView)
        {

                while (true)
                {
                    Thread.Sleep(5000);
                    progressView.ProgressValueInc();
                }



        }
    }
}
