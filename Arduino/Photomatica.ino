const int buttonPin = 5;
bool lastButtonState = HIGH;

const int leds[] = {2, 3, 4};
const int ledCount = sizeof(leds) / sizeof(leds[0]);

void setup() {
  pinMode(buttonPin, INPUT_PULLUP);
  Serial.begin(9600);

  for (int i = 0; i < ledCount; i++) {
    pinMode(leds[i], OUTPUT);
    digitalWrite(leds[i], LOW);
  }
}

void loop() {
  // Check fysieke knop
  bool currentState = digitalRead(buttonPin);
  if (lastButtonState == HIGH && currentState == LOW) {
    Serial.println("BUTTON");
    delay(300); // debounce
  }
  lastButtonState = currentState;

  // Lees seriële input
  if (Serial.available()) {
    String input = Serial.readStringUntil('\n');
    input.trim();

    if (input.startsWith("COUNTDOWN;")) {
      int intervalMs = input.substring(10).toInt();

      // LED countdown met aanhouden aan
      for (int i = 0; i < ledCount; i++) {
        digitalWrite(leds[i], HIGH);
        delay(intervalMs);
      }

      // Zet alle LED's uit na countdown
      for (int i = 0; i < ledCount; i++) {
        digitalWrite(leds[i], LOW);
      }

      delay(100);
      Serial.println("READY");
    }
  }
}
