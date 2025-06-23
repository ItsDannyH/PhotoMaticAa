const int ledPins[] = {2, 3, 4}; // LED pins
const int buttonPin = 5;
bool lastButtonState = LOW;
unsigned long lastDebounceTime = 0;
const unsigned long debounceDelay = 50;

int countdownDelayMs = 1000; // tijd tussen LEDs (komt van PC)
int pictureDelayMs = 1000;   // tijd na laatste LED tot foto

void setup() {
  Serial.begin(9600);
  for (int i = 0; i < 3; i++) {
    pinMode(ledPins[i], OUTPUT);
    digitalWrite(ledPins[i], LOW);
  }
  pinMode(buttonPin, INPUT_PULLUP);
}

void loop() {
  // Check op knopdruk
  int reading = digitalRead(buttonPin);
  if (reading == LOW && lastButtonState == HIGH && (millis() - lastDebounceTime) > debounceDelay) {
    lastDebounceTime = millis();
    handleCountdown();
  }
  lastButtonState = reading;

  // Check op seriële input van PC
  if (Serial.available()) {
    String cmd = Serial.readStringUntil('\n');
    cmd.trim();
    if (cmd.startsWith("COUNTDOWN")) {
      // Parse optioneel extra interval: "COUNTDOWN 1000 200"
      int space1 = cmd.indexOf(' ');
      if (space1 > 0) {
        int space2 = cmd.indexOf(' ', space1 + 1);
        countdownDelayMs = cmd.substring(space1 + 1, space2).toInt();
        pictureDelayMs = cmd.substring(space2 + 1).toInt();
      }
      handleCountdown();
    }
  }
}

void handleCountdown() {
  for (int i = 0; i < 3; i++) {
    digitalWrite(ledPins[i], HIGH);
    delay(countdownDelayMs);
  }

  delay(pictureDelayMs); // Wacht na laatste LED
  Serial.println("READY");

  // LEDs uit na foto
  for (int i = 0; i < 3; i++) {
    digitalWrite(ledPins[i], LOW);
  }
}
