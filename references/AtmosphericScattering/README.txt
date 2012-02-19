
- Demo de Componente Skydome con atmospheric scattering - 

Este es un sencillo componente para a�adir un skydome a tus juegos, 
implementando una simulaci�n de atmospheric scattering (sin llegar
en absoluto a la complejidad de c�lculo de otros m�todos mucho m�s
precisos y correctos f�sicamente como el de Sean O'Neill).



- Controles -

Tecla Espacio: cambia el tipo de animaci�n del sol entre las siguientes

	� Autom�tica: muestra el ciclo d�a/noche
	� Hora actual: se calcula la posici�n del sol seg�n la hora
		"real" del sistema (contando que amanece a las 6h y
		anochece a las 18h)
	� Manual: con las feclas arriba/abajo se avanza/retrasa la
		hora del d�a

Teclas W/S/A/D: traslada la c�mara

Rat�n: gira la c�mara



- Notas - 

Se puede modificar f�cilmente el componente para no tener que usar
FreeCamera, bastar�a pasar por referencia la informaci�n que necesita,
que es la posici�n de la c�mara y las matrices View y Projection.

Tambi�n es conveniente jugar con los par�metros del skydome para 
obtener el resultado que m�s guste.



- Agradecimientos - 

Gracias a Javier Cant�n Ferrero por su siempre �til componente 
FreeCamera. Ahora ya molo. ;)



Para cualquier comentario:

goefuika@gmail.com


Saludos!


www.codeplex.com/XNACommunity