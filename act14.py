import serial
import mysql.connector
from mysql.connector import Error
from datetime import datetime
import time

class ArduinoDatabaseConnector:
    def __init__(self):
    
        self.port = 'COM4' 
        self.arduino = serial.Serial(self.port, self.baud_rate, timeout=1)
        
       
        self.connection = None
        try:
            self.connection = mysql.connector.connect(
                host='localhost',
                database='formulario',
                user='root',
                password='1993'
            )
            if self.connection.is_connected():
                print("Conectado a la base de datos")
        except Error as e:
            print(f"Error al conectar con la base de datos: {e}")

    def connect_to_arduino(self):
        if not self.arduino.is_open:
            self.arduino.open()
            print("Conectado al Arduino en", self.port)

    def disconnect_from_arduino(self):
        if self.arduino.is_open:
            self.arduino.close()
            print("Desconectado del Arduino")

    def send_data_to_arduino(self, data):
        if self.arduino.is_open:
            self.arduino.write(data.encode())
            print("Datos enviados al Arduino:", data)
        else:
            print("Conéctate al Arduino antes de enviar datos.")

    def read_from_arduino(self):
        if self.arduino.is_open:
            line = self.arduino.readline().decode('utf-8').strip()
            if line:
                print("Datos recibidos del Arduino:", line)
                self.process_received_data(line)
        else:
            print("Conéctate al Arduino antes de leer datos.")

    def process_received_data(self, data):
       
        if data.startswith("TEMP:"):
            temperature = data.replace("TEMP:", "").strip()
            try:
                temperature = float(temperature)
                print("Temperatura:", temperature)
                self.save_data_to_database("temperatura", temperature)
            except ValueError:
                print("Error en el formato de temperatura:", data)

        elif data.startswith("SERVO:"):
            servo_state = data.replace("SERVO:", "").strip()
            print("Estado del Servomotor:", servo_state)
            self.save_data_to_database("servo", servo_state)

    def save_data_to_database(self, tipo, valor):
        try:
            if self.connection.is_connected():
                cursor = self.connection.cursor()
                query = "INSERT INTO datos_sensor (tipo, valor, fecha) VALUES (%s, %s, %s)"
                cursor.execute(query, (tipo, str(valor), datetime.now()))
                self.connection.commit()
                print("Datos guardados en la base de datos.")
        except Error as e:
            print(f"Error al guardar datos en la base de datos: {e}")
        finally:
            if self.connection.is_connected():
                cursor.close()

    def close_database_connection(self):
        if self.connection.is_connected():
            self.connection.close()
            print("Conexión a la base de datos cerrada.")


if __name__ == "__main__":
    connector = ArduinoDatabaseConnector()
    connector.connect_to_arduino()

    try:
        while True:
            connector.read_from_arduino()
            time.sleep(1)  
    except KeyboardInterrupt:
        print("Interrupción del programa.")
    finally:
        connector.disconnect_from_arduino()
        connector.close_database_connection()
