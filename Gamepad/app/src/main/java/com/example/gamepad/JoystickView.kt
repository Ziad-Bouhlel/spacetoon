package com.example.gamepad

import android.content.Context
import android.graphics.Canvas
import android.graphics.Color
import android.graphics.Paint
import android.graphics.PointF
import android.util.AttributeSet
import android.view.MotionEvent
import android.view.View

class JoystickView(context: Context, attrs: AttributeSet) : View(context, attrs) {

    private var baseRadius = 150f
    private var hatRadius = 80f
    private val center = PointF(0f, 0f)
    private val hat = PointF(0f, 0f)
    private var listener: ((Float, Float) -> Unit)? = null

    private val basePaint = Paint().apply {
        color = Color.DKGRAY
        style = Paint.Style.FILL
    }
    private val hatPaint = Paint().apply {
        color = Color.LTGRAY
        style = Paint.Style.FILL
    }

    init {
        setOnTouchListener { _, event ->
            if (event.action == MotionEvent.ACTION_UP) {
                resetHat()
            } else {
                updateHat(event.x, event.y)
            }
            invalidate() // Redessine le joystick
            true
        }
    }

    override fun onSizeChanged(w: Int, h: Int, oldw: Int, oldh: Int) {
        super.onSizeChanged(w, h, oldw, oldh)
        center.set(width / 2f, height / 2f)
        resetHat()
    }

    override fun onDraw(canvas: Canvas) {
        super.onDraw(canvas)
        // Dessine le fond
        canvas.drawCircle(center.x, center.y, baseRadius, basePaint)
        // Dessine le chapeau du joystick
        canvas.drawCircle(hat.x, hat.y, hatRadius, hatPaint)
    }

    private fun updateHat(x: Float, y: Float) {
        val dx = x - center.x
        val dy = y - center.y
        val distance = Math.hypot(dx.toDouble(), dy.toDouble())
        val maxDistance = baseRadius

        if (distance < maxDistance) {
            hat.set(x, y)
        } else {
            val ratio = maxDistance / distance
            hat.set(center.x + dx * ratio.toFloat(), center.y + dy * ratio.toFloat())
        }

        // Calcul des valeurs normalisées entre -1 et 1
        val normalizedX = (hat.x - center.x) / maxDistance
        val normalizedY = (hat.y - center.y) / maxDistance
        listener?.invoke(normalizedX, normalizedY)
    }

    private fun resetHat() {
        hat.set(center.x, center.y)
        listener?.invoke(0f, 0f) // Retour au centre
    }

    fun setOnMoveListener(listener: (Float, Float) -> Unit) {
        this.listener = listener
    }
}
