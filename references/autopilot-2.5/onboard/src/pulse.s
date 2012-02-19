/*
 * $Id: pulse.s,v 2.0 2002/09/22 02:10:18 tramm Exp $
 *
 * Optimized pulse width measuring routine.  Should require only a few
 * microseconds per call.  One pulse = 1.25 microseconds.
 */

.text


.global pulse_width
pulse_width:
	ldi	r24, lo8(0)
	ldi	r25, lo8(0)

wait_for_lo:
	adiw	r24, 1			/* 2 */
	in	r19, 16			/* 1 */
	sbrs	r19, 2			/* 1 */
	rjmp	wait_for_lo		/* 1 */

wait_for_hi:
	adiw	r24, 1			/* 2 */
	in	r19, 16			/* 1 */
	sbrc	r19, 2			/* 1 */
	rjmp	wait_for_hi		/* 1 */

	ret


.global wait_for_sync
wait_for_sync:
loop:
	in	r19, 16
	sbrc	r19, 2
	rjmp	loop

	call	pulse_width
	subi	r24, lo8( 4096 )
	sbci	r25, hi8( 4096 )
	brlo	loop

	ret
	
