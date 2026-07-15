Importante el archivo "PROJECT_README" contiene las reglas y especificaciones y otras consideración con respecto al proyecto.

Ideas en bruto que deben implementarse segun area:

Corregir lo siguiente en AvaloniaUI:

Algunos botones(y todo lo relacionado al respectivo boton) en AvaloniaUI no están funcionando

Con respecto a WebUI corregir:

Algunos botones que estan en AvaloniaUI no están presentes en WebUI

Los títulos de las diferentes ventanas siempre deben estar centrados en el medio, y debajo debe seguir su contenido respectivo

La ventana de crear Producto "http://localhost:5279/productos/crear" debe usar correctamente el espacio de la ventana y estar centrado

La ventana de Ventas y Historial de ventas deben estar separadas, pues tienen usos diferentes entre si, donde historial de ventas debería ser "http://localhost:5279/ventas/historial"

Importante hay ventanas en AvaloniaUI que aun no existen en WebUI o no están correctamente implementadas, se deben verificar cuales son y añadirlas a la lista de ventanas con su correcto funcionamiento correcto

La ventana del "Dashboard" no solo debe mostrar tarjetas de métricas, también otras consideraciones como una tabla de los 10 productos mas vendidos, con la posibilidad de ordenar en base a los estados de "ingresos" (que producto genera mas ventas) y "unidades vendidas" (que producto se vendio mas en cantidad), además tendrá un estado de rango de fecha de los últimos 3 meses como default, que podrá modificarse manualmente y mantenerse entre sesiones y tambien se mostrara una grafica de lineas de los ingresos diarios con un rango de fecha igual al de la tabla

En las diferentes ventanas donde se mencione a algún producto, y en las respectivas tablas donde se muestren los productos, debe implementarse que en el lado izquierdo pero en la misma regilla (espacio) del nombre del producto debe haber una imagen correspondiente al producto o en caso no tener imagen se mostrara un icono o imagen clásico de "producto sin imagen", pero esta imagen que este alado del producto debe ser una imagen reducida de tamaño, en si que no expanda el tamaño de la fila y además debe interactuarse con la imagen y al hacerle click debe mostrarse como un pop-up (ventana emergente que se superpone a la "ventana", y al momento de mostrarse la imagen expandida esta debe llegar al 70% del área de la ventana

Cambiar la ubicación de la "versión", para que este debajo del nombre del sistema "TeejoshSystem" en la seccion lista y ventanas y eliminar el texto que actualmente emnciona "Canal Web TeejoshSystem 0.2.0"

En la ventana de "Ventas" debe repartirse el espacio de forma dinámica en base a un panel dividido vertical con separador redimensionable

En todas las ventanas del sistema no debe existir una barra de despalzamiento horizontal, todos los elementos de cada ventana deben distribuirse dinamicamente en base al tamaño del navegador usado

En todas las tablas de todas las ventanas, donde exista la columna nombre de producto debe existir un comportamiento donde el nombre del producto se muestre dinamicamente en base al tamaño de la ventana y del tamaño de la tabla donde se encuentre de tal forma que reduciremos el nombre a un formato como:"nombre_recortado" + "..."

Corregir la mención de "$"(dólares) a "S/" (soles)

Actualmente en las tablas de productos de las diferentes ventanas, la columna de stock solo muestra el estado "disponible" y "sin stock", lo que no ayuda a entender las cantidades reales que existen en ese momento, asi que se harán las siguientes modificaciones en cuanto a este tema: primero se le añadira un icono de un ojito alado de la palabra "STOCK", y cuando el ojito este abierto entonces la presentación de los valores de stock de la diferentes filas será con las cantidades existentes que están en es momento de cada producto, y cuando el ojito este cerrado entonces la presentación del estado de las existencias se mantendrá con "disponible" y "sin stock" y además solo cuando se haga click a ese estado ("disponible" y "sin stock") se reemplazara el estado por la cantidad de existencias del respectivo producto en ese momento
