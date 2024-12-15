// MainActivity.kt
package com.example.sensortransfer

import android.hardware.Sensor
import android.hardware.SensorEvent
import android.hardware.SensorEventListener
import android.hardware.SensorManager
import androidx.appcompat.app.AppCompatActivity
import android.os.Bundle
import android.widget.TextView
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import java.io.OutputStream
import java.net.Socket

class MainActivity : AppCompatActivity(), SensorEventListener {

    private lateinit var sensorManager: SensorManager
    private var accelerometer: Sensor? = null
    private lateinit var accelerometerDataTextView: TextView

    // Server configuration
    private val serverIP = "192.168.1.100" // Replace with your server's IP address
    private val serverPort = 12345          // Replace with your server's port

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        accelerometerDataTextView = findViewById(R.id.accelerometerDataTextView)

        // Initialize sensor manager and accelerometer
        sensorManager = getSystemService(SENSOR_SERVICE) as SensorManager
        accelerometer = sensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER)

        if (accelerometer == null) {
            accelerometerDataTextView.text = "Accelerometer not available"
        }
    }

    override fun onResume() {
        super.onResume()
        accelerometer?.also {
            sensorManager.registerListener(this, it, SensorManager.SENSOR_DELAY_NORMAL)
        }
    }

    override fun onPause() {
        super.onPause()
        sensorManager.unregisterListener(this)
    }

    override fun onSensorChanged(event: SensorEvent?) {
        if (event?.sensor?.type == Sensor.TYPE_ACCELEROMETER) {
            val x = event.values[0]
            val y = event.values[1]
            val z = event.values[2]

            val data = "X: $x, Y: $y, Z: $z"
            accelerometerDataTextView.text = data

            // Send data to the server
            CoroutineScope(Dispatchers.IO).launch {
                sendDataToServer(data)
            }
        }
    }

    override fun onAccuracyChanged(sensor: Sensor?, accuracy: Int) {
        // Do nothing
    }

    private fun sendDataToServer(data: String) {
        try {
            val socket = Socket(serverIP, serverPort)
            val outputStream: OutputStream = socket.getOutputStream()
            outputStream.write((data + "\n").toByteArray())
            outputStream.flush()
            socket.close()
        } catch (e: Exception) {
            e.printStackTrace()
        }
    }
}