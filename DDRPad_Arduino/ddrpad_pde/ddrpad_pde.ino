// The pins to use.  On the VGA cable, 3 is ground. 
int g_UpButtonPin =  2; 	// VGA Pin 14
int g_DownButtonPin =  3; 	// VGA Pin 13
int g_LeftButtonPin =  4; 	// VGA Pin 12
int g_RightButtonPin =  5; 	// VGA Pin 11
int g_XButtonPin =  6;
int g_OButtonPin =  7;

void setup() {
  // Set our 6 inputs to input mode and
  //    turn on their 20k pullup resistors on
  pinMode(g_UpButtonPin, INPUT);
  digitalWrite(g_UpButtonPin, HIGH);
  pinMode(g_DownButtonPin, INPUT);
  digitalWrite(g_DownButtonPin, HIGH);
  pinMode(g_LeftButtonPin, INPUT);
  digitalWrite(g_LeftButtonPin, HIGH);
  pinMode(g_RightButtonPin, INPUT);
  digitalWrite(g_RightButtonPin, HIGH);
  pinMode(g_XButtonPin, INPUT);
  digitalWrite(g_XButtonPin, HIGH);
  pinMode(g_OButtonPin, INPUT);
  digitalWrite(g_OButtonPin, HIGH);
  
  Serial.begin(9600);
}

void loop() {
  // Send the state of all the buttons
  //    They are in order and 0 is up, 1 is down.
  for (int i = g_UpButtonPin; i <= g_OButtonPin; i++) {
    if (digitalRead(i) == LOW) // Active low
      Serial.print('1');  
    else
      Serial.print('0');
    
    Serial.print('\0');
  }
  Serial.println();
  Serial.flush();
  delay(10);
}

