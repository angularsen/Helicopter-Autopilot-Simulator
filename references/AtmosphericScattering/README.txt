
- Demo de Componente Skydome con atmospheric scattering - 

Este es un sencillo componente para añadir un skydome a tus juegos, 
implementando una simulación de atmospheric scattering (sin llegar
en absoluto a la complejidad de cálculo de otros métodos mucho más
precisos y correctos físicamente como el de Sean O'Neill).



- Controles -

Tecla Espacio: cambia el tipo de animación del sol entre las siguientes

	· Automática: muestra el ciclo día/noche
	· Hora actual: se calcula la posición del sol según la hora
		"real" del sistema (contando que amanece a las 6h y
		anochece a las 18h)
	· Manual: con las feclas arriba/abajo se avanza/retrasa la
		hora del día

Teclas W/S/A/D: traslada la cámara

Ratón: gira la cámara



- Notas - 

Se puede modificar fácilmente el componente para no tener que usar
FreeCamera, bastaría pasar por referencia la información que necesita,
que es la posición de la cámara y las matrices View y Projection.

También es conveniente jugar con los parámetros del skydome para 
obtener el resultado que más guste.



- Agradecimientos - 

Gracias a Javier Cantón Ferrero por su siempre útil componente 
FreeCamera. Ahora ya molo. ;)



Para cualquier comentario:

goefuika@gmail.com


Saludos!


www.codeplex.com/XNACommunity