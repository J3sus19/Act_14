#include <LiquidCrystal_I2C.h>
#include <Servo.h>

#define TEMP_PIN A0   // Pin para el sensor de temperatura LM35
#define SERVO_PIN 9   // Pin para el servomotor

LiquidCrystal_I2C lcd(0x27, 16, 2); // Dirección I2C 0x27 para una pantalla de 16x2
Servo servoMotor;

float temperature = 0.0;
String servoState = "OFF";

void setup() {
  Serial.begin(9600);

  // Configuración del LCD
  lcd.begin(16, 2);
  lcd.backlight();
  lcd.setCursor(0, 0);
  lcd.print("Iniciando...");

  // Configuración del servomotor
  servoMotor.attach(SERVO_PIN);
  servoMotor.write(0);  // Posición inicial del servomotor

  delay(1000);
  lcd.clear();
}

void loop() {
  // Leer temperatura
  int tempValue = analogRead(TEMP_PIN);
  temperature = (tempValue * (5.0 / 1023.0)) * 100.0; // Conversión a grados Celsius para LM35

  // Actualizar LCD
  lcd.setCursor(0, 0);
  lcd.print("Temp: ");
  lcd.print(temperature);
  lcd.print(" C   ");

  // Control del servomotor basado en temperatura
  if (temperature > 25) {
    servoMotor.write(90);      // Mover el servomotor a 90 grados si la temperatura es alta
    servoState = "ON";
  } else {
    servoMotor.write(0);       // Mantener en 0 grados si la temperatura es baja
    servoState = "OFF";
  }

  
  lcd.setCursor(0, 1);
  lcd.print("Servo: ");
  lcd.print(servoState);
  lcd.print("   ");

  
  Serial.print("TEMP:");
  Serial.println(temperature);
  Serial.print("SERVO:");
  Serial.println(servoState);

  delay(1000); 
}
