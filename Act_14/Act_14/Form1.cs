using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Act_14
{
    public partial class Form1 : Form
    {
        public SerialPort ArduinoPort { get; }
        public MySqlConnection _connection;
        private string connectionString = "server=localhost;database=formulario;uid=root;pwd=1993";

        public Form1()
        {
            InitializeComponent();

            _connection = new MySqlConnection(connectionString);


            ArduinoPort = new SerialPort("COM4", 9600);
            ArduinoPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }

      
        private void btnConectar_Click(object sender, EventArgs e)
        {
            if (!ArduinoPort.IsOpen)
            {
                try
                {
                    ArduinoPort.Open();
                    MessageBox.Show("Conectado al Arduino en COM4.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al conectar: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("El puerto ya está abierto.");
            }
        }


        private void btnDesconectar_Click_1(object sender, EventArgs e)
        {
            if (ArduinoPort.IsOpen)
            {
                try
                {
                    ArduinoPort.Close();
                    MessageBox.Show("Desconectado del Arduino.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al desconectar: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("El puerto ya está cerrado.");
            }
        }

        private void btnEnviar_Click_1(object sender, EventArgs e)
        {
            if (ArduinoPort.IsOpen)
            {
                try
                {
                    string dataToSend = "DATOS";
                    ArduinoPort.WriteLine(dataToSend);
                    MessageBox.Show("Datos enviados al Arduino.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al enviar datos: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Conéctate al Arduino antes de enviar datos.");
            }
        }


        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string data = ArduinoPort.ReadLine();
            this.Invoke(new MethodInvoker(delegate
            {

                ProcessReceivedData(data);
            }));
        }

        
        private void ProcessReceivedData(string data)
        {

            if (data.StartsWith("TEMP:"))
            {
               
                string tempString = data.Replace("TEMP:", "");
                if (double.TryParse(tempString, out double temperature))
                {

                    Console.WriteLine("Temperatura: " + temperature);

                    SaveDataToDatabase("temperatura", temperature.ToString());
                }
            }
            else if (data.StartsWith("SERVO:"))
            {
                string servoState = data.Replace("SERVO:", "").Trim();

                Console.WriteLine("Estado del Servomotor: " + servoState);
    
                SaveDataToDatabase("servo", servoState);
            }
        }

    
        private void SaveDataToDatabase(string tipo, string valor)
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                string query = "INSERT INTO datos_sensor (tipo, valor, fecha) VALUES (@tipo, @valor, @fecha)";
                MySqlCommand cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.Parameters.AddWithValue("@valor", valor);
                cmd.Parameters.AddWithValue("@fecha", DateTime.Now);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Datos guardados en la base de datos.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar datos en la base de datos: " + ex.Message);
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();
            }
        }

        
    }
}
