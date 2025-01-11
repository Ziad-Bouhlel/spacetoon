package com.example.gamepad

import android.annotation.SuppressLint
import android.content.Context
import android.graphics.Canvas
import android.graphics.Color
import android.graphics.Paint
import android.graphics.PointF
import android.util.AttributeSet
import android.view.MotionEvent
import android.view.View

@SuppressLint("ClickableViewAccessibility")
class PadView(context: Context, attrs: AttributeSet) : View(context, attrs) {

    private var padRadius = 150f
    private val padCenter = PointF(0f, 0f)
    private var listener: ((Float, Float) -> Unit)? = null

    private val padPaint = Paint().apply {
        color = Color.CYAN
        style = Paint.Style.FILL
    }

    private val minHeightLimit = 0.3f

    init {
        post {
            padCenter.set(width / 2f, height / 2f)
            invalidate()
        }

        setOnTouchListener { _, event ->
            if (!isTouchInsidePad(event)) {
                // L'événement ne concerne pas le PadView, permettre au parent de le gérer
                return@setOnTouchListener false
            }

            when (event.action) {
                MotionEvent.ACTION_DOWN, MotionEvent.ACTION_MOVE -> {
                    parent.requestDisallowInterceptTouchEvent(true)
                    if (isWithinBounds(event.x, event.y)) {
                        updatePad(event.x, event.y)
                    }
                }
                MotionEvent.ACTION_UP -> {
                    parent.requestDisallowInterceptTouchEvent(false)
                }
            }
            invalidate() // Redessiner le pad
            true
        }
    }

    override fun onDraw(canvas: Canvas) {
        super.onDraw(canvas)
        // Dessine le pad mobile
        canvas.drawCircle(padCenter.x, padCenter.y, padRadius, padPaint)
    }

    private fun updatePad(x: Float, y: Float) {
        // Déplace le pad à la position touchée
        val boundedY = y.coerceAtLeast(height * minHeightLimit).coerceAtMost(height.toFloat())
        padCenter.set(x.coerceIn(0f, width.toFloat()), boundedY)

        // Calcul des valeurs normalisées entre -1 et 1
        val normalizedX = (x - width / 2f) / (width / 2f)
        val normalizedY = (y - height / 2f) / (height / 2f)
        listener?.invoke(normalizedX, normalizedY)
    }

    private fun isWithinBounds(x: Float, y: Float): Boolean {
        // Vérifie si le clic est dans la zone définie pour le pad
        return x >= 0 && x <= width && y >= height * minHeightLimit && y <= height
    }

    private fun isTouchInsidePad(event: MotionEvent): Boolean {
        // Vérifie si le toucher est à l'intérieur du pad
        val dx = event.x - padCenter.x
        val dy = event.y - padCenter.y
        return dx * dx + dy * dy <= padRadius * padRadius
    }

    private fun resetPad() {
        // Ramener le pad au centre de l'écran
        padCenter.set(width / 2f, height / 2f)
        listener?.invoke(0f, 0f) // Position neutre
    }

    fun setOnMoveListener(listener: (Float, Float) -> Unit) {
        this.listener = listener
    }
}
