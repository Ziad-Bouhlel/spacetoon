package com.example.gamepad

import android.annotation.SuppressLint
import android.os.Bundle
import android.widget.EditText
import android.widget.Switch
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import org.json.JSONObject
import java.io.PrintWriter
import java.net.Socket

class MainActivity : AppCompatActivity() {
    private lateinit var joystickView: JoystickView
    private lateinit var playerSwitch: Switch
    private lateinit var coeffXInput: EditText
    private lateinit var coeffYInput: EditText

    private val SERVER_IP = "192.168.1.16" // Remplace par l'adresse IP de ton serveur
    private val SERVER_PORT = 5000 // Remplace par le port de ton serveur
    private var socket: Socket? = null
    private var writer: PrintWriter? = null

    private var coeffX = 1.0f
    private var coeffY = 1.0f
    private var currentPlayer = 1 // Par défaut Joueur 1

    @SuppressLint("ClickableViewAccessibility")
    protected override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

//        val buttonUp: Button = findViewById(R.id.buttonUp)
//        val buttonDown: Button = findViewById(R.id.buttonDown)
//        val buttonLeft: Button = findViewById(R.id.buttonLeft)
//        val buttonRight: Button = findViewById(R.id.buttonRight)

        // Initialisation des vues
        joystickView = findViewById(R.id.joystickView)
        playerSwitch = findViewById(R.id.playerSwitch)
        coeffXInput = findViewById(R.id.coeffX)
        coeffYInput = findViewById(R.id.coeffY)

        connectToServer()



//        buttonUp.setOnTouchListener { _, event ->
//            handleButtonPress(event, 0, 1) // Déplacement vers le haut (x = 0, y = 1)
//        }
//        buttonDown.setOnTouchListener { _, event ->
//            handleButtonPress(event, 0, -1) // Déplacement vers le bas (x = 0, y = -1)
//        }
//        buttonLeft.setOnTouchListener { _, event ->
//            handleButtonPress(event, -1, 0) // Déplacement vers la gauche (x = -1, y = 0)
//        }
//        buttonRight.setOnTouchListener { _, event ->
//            handleButtonPress(event, 1, 0) // Déplacement vers la droite (x = 1, y = 0)
//        }


        // Gestion des mouvements du joystick
        joystickView.setOnMoveListener { x, y ->
            val adjustedX = x * coeffX
            val adjustedY = y * coeffY
            sendVector(adjustedX, adjustedY)
        }

        // Gestion du Switch Joueur 1 ou Joueur 2
        playerSwitch.setOnCheckedChangeListener { _, isChecked ->
            currentPlayer = if (isChecked) 1 else 2
            playerSwitch.text = if (isChecked) "Joueur 1" else "Joueur 2"
        }

        // Gestion des coefficients X et Y
        coeffXInput.setOnFocusChangeListener { _, _ ->
            coeffX = coeffXInput.text.toString().toFloatOrNull() ?: 1.0f
        }
        coeffYInput.setOnFocusChangeListener { _, _ ->
            coeffY = coeffYInput.text.toString().toFloatOrNull() ?: 1.0f
        }

    }

//    private fun handleButtonPress(event: MotionEvent, x: Int, y: Int): Boolean {
//        if (event.action == MotionEvent.ACTION_DOWN) {
//            sendVector(x, y) // Envoie le vecteur (x, y) lorsque le bouton est pressé
//        } else if (event.action == MotionEvent.ACTION_UP) {
//            sendVector(0, 0) // Stoppe le déplacement en envoyant (0, 0)
//        }
//        return true
//    }


    private fun connectToServer(){
        Thread {
            try {
                socket = Socket(SERVER_IP, SERVER_PORT)
                writer = PrintWriter(socket!!.getOutputStream(), true)
                runOnUiThread {
                    Toast.makeText(
                        this@MainActivity,
                        "Connected to server",
                        Toast.LENGTH_SHORT
                    ).show()
                }
            } catch (e: Exception) {
                runOnUiThread {
                    Toast.makeText(
                        this@MainActivity,
                        "Connection failed: " + e.message,
                        Toast.LENGTH_SHORT
                    ).show()
                }
            }
        }.start()
    }

    private fun sendVector(x: Float, y: Float) {
        Thread {
            try {
                if (writer != null) {
                    // Création d'un objet JSON pour le vecteur
                    val json = JSONObject()

                    json.put("timestamp",System.currentTimeMillis())
                    json.put("joueur", currentPlayer) // Ajoute le joueur sélectionné
                    json.put("x", x)
                    json.put("y", y)
                    writer!!.println(json.toString()) // Envoi de l'objet JSON sous forme de chaîne
                }
            } catch (e: Exception) {
                runOnUiThread {
                    Toast.makeText(
                        this@MainActivity,
                        "Failed to send vector: " + e.message,
                        Toast.LENGTH_SHORT
                    ).show()
                }
            }
        }.start()
    }

    override fun onDestroy() {
        super.onDestroy()
        try {
            writer?.close()
            socket?.close()
        } catch (e: Exception) {
            e.printStackTrace()
        }
    }
}