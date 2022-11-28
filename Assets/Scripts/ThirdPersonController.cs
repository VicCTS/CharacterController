using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    private CharacterController controller;
    private Animator anim;
    public Transform cam;
    public Transform LookAtTransform;

    //variables para controlar velocidad, altura de salto y gravedad
    public float speed = 5;
    public float jumpHeight = 1;
    public float gravity = -9.81f;
    [SerializeField]private float pushStrength = 4f;

    //variables para el ground sensor
    public bool isGrounded;
    public Transform groundSensor;
    public float sensorRadius = 0.1f;
    public LayerMask ground;
    private Vector3 playerVelocity;

    //variables para rotacion del personaje
    private float turnSmoothVelocity;
    public float turnSmoothTime = 0.1f;

    //variables para el movimiento del raton con virtual camera
    public Cinemachine.AxisState xAxis;
    public Cinemachine.AxisState yAxis;

    public GameObject[] cameras;

    //Variables para coger objetos
    public GameObject objectToPick;
    [SerializeField]private GameObject pickedObject;
    [SerializeField]Transform interactionZone;
    
    // Start is called before the first frame update
    void Start()
    {
        //Asignamos el character controller a su variable
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();

        //Con esto podemos esconder el icono del raton para que no moleste
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //Llamamos la funcion de movimiento
        //Movement();
        MovementTPS();
        //MovementTPS2();
        
        //Lamamaos la funcion de salto
        Jump();
        PickObjects();
    }
#region FuncionesDeMovimiento
    void Movement()
    {
        //Creamos un Vector3 y en los ejes X y Z le asignamos los inputs de movimiento
        Vector3 move = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        if(move != Vector3.zero)
        {
            //Creamos una variable float para almacenar la posicion a la que queremos que mire el personaje
            //Usamos la funcion Atan2 para calcular el angulo al que tendra que mirar nuestro personaje
            //lo multiplicamos por Rad2Deg para que nos de el valor en grados y le sumamos la rotacion de la camara en Y para que segund donde mire la camara afecte a la rotacion
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            //Usamos un SmoothDamp para que nos haga una transicion entre el angulo actual y al que queremos llegar
            //de esta forma no nos rotara de golpe al personaje
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            //le aplicamos la rotacion al personaje
            transform.rotation = Quaternion.Euler(0, angle, 0);

            //Creamos otro Vector3 el cual multiplicaremos el angulo al que queremos que mire el personaje por un vector hacia delante
            //para que el personaje camine en la direccion correcta a la que mira
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            //Funcion del character controller a la que le pasamos el Vector que habiamos creado y lo multiplicamos por la velocidad para movernos
            controller.Move(moveDirection.normalized * speed * Time.deltaTime);
        }
    }

    //Movimiento TPS con Freelook camera
    void MovementTPS()
    {
        //Creamos un Vector3 y en los ejes X y Z le asignamos los inputs de movimiento
        Vector3 move = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        anim.SetFloat("VelZ", z);
        anim.SetFloat("VelX", x);

        if(move != Vector3.zero)
        {
            //Creamos una variable float para almacenar la posicion a la que queremos que mire el personaje
            //Usamos la funcion Atan2 para calcular el angulo al que tendra que mirar nuestro personaje
            //lo multiplicamos por Rad2Deg para que nos de el valor en grados y le sumamos la rotacion de la camara en Y para que segund donde mire la camara afecte a la rotacion
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            //Usamos un SmoothDamp para que nos haga una transicion entre el angulo actual y el de la camara
            //de esta forma no nos rotara de golpe al personaje
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, cam.eulerAngles.y, ref turnSmoothVelocity, turnSmoothTime);
            //le aplicamos la rotacion al personaje
            transform.rotation = Quaternion.Euler(0, angle, 0);

            //Creamos otro Vector3 el cual multiplicaremos el angulo al que queremos que mire el personaje por un vector hacia delante
            //para que el personaje camine en la direccion correcta a la que mira
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            //Funcion del character controller a la que le pasamos el Vector que habiamos creado y lo multiplicamos por la velocidad para movernos
            controller.Move(moveDirection.normalized * speed * Time.deltaTime);
        }
    }

    //Movimiento TPS con virtaul camera
    void MovementTPS2()
    {
        //Creamos un Vector3 y en los ejes X y Z le asignamos los inputs de movimiento
        Vector3 move = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        //Actualizamos los inputs del raton
        xAxis.Update(Time.deltaTime);
        yAxis.Update(Time.deltaTime);

        //Hacemos rotar al personaje en el eje Y dependiendo del valor X del raton
        transform.rotation = Quaternion.Euler(0, xAxis.Value, 0);
        //Hacemos rotar la camara en el eje X dependiendo del valor del raton en el eje Y
        LookAtTransform.eulerAngles = new Vector3(yAxis.Value, xAxis.Value, LookAtTransform.eulerAngles.z);
        //LookAtTransform.rotation = Quaternion.Euler(yAxis.Value, xAxis.Value, LookAtTransform.eulerAngles.z);

        //Si pulsamos el boton de apuntar activamos y desactivamos las camaras correspondientes
        if(Input.GetButton("Fire2"))
        {
            cameras[0].SetActive(false);
            cameras[1].SetActive(true);
        }
        else
        {
            cameras[0].SetActive(true);
            cameras[1].SetActive(false);
        }


        if(move != Vector3.zero)
        {
            //Creamos una variable float para almacenar la posicion a la que queremos que mire el personaje
            //Usamos la funcion Atan2 para calcular el angulo al que tendra que mirar nuestro personaje
            //lo multiplicamos por Rad2Deg para que nos de el valor en grados y le sumamos la rotacion de la camara en Y para que segund donde mire la camara afecte a la rotacion
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            //Usamos un SmoothDamp para que nos haga una transicion entre el angulo actual y el de la camara
            //de esta forma no nos rotara de golpe al personaje
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, cam.eulerAngles.y, ref turnSmoothVelocity, turnSmoothTime);
            
            //Creamos otro Vector3 el cual multiplicaremos el angulo al que queremos que mire el personaje por un vector hacia delante
            //para que el personaje camine en la direccion correcta a la que mira
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            //Funcion del character controller a la que le pasamos el Vector que habiamos creado y lo multiplicamos por la velocidad para movernos
            controller.Move(moveDirection.normalized * speed * Time.deltaTime);
        }
    }
#endregion

#region FuncionDeSalto
    //Funcion de salto y gravedad
    void Jump()
    {
        //Le asignamos a la boleana isGrounded su valor dependiendo del CheckSpher
        //CheckSphere crea una esfera pasandole la poscion, radio y layer con la que queremos que interactue
        //si la esfera entra en contacto con la capa que le digamos convertira nuestra boleana en true y si no entra en contacto en false
        isGrounded = Physics.CheckSphere(groundSensor.position, sensorRadius, ground);

        //Si estamos en el suelo y playervelocity es menor que 0 hacemos que le vuelva a poner el valor a 0
        //esto es para evitar que siga aplicando fuerza de gravedad cuando estemos en el suelo y evitar comportamientos extraños
        if(isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0;
        }

        //si estamos en el suelo y pulasamos el imput de salto hacemos que salte el personaje
        if(isGrounded && Input.GetButtonDown("Jump"))
        {
            anim.SetBool("IsJumping", true);

            //Formula para hacer que los saltos sean de una altura concreta
            //la altura depende del valor de jumpHeight 
            //Si jumpHeigt es 1 saltara 1 metro de alto
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity); 
        }

        //a playervelocity.y le iremos sumando el valor de la gravedad
        playerVelocity.y += gravity * Time.deltaTime;
        //como playervelocity en el eje Y es un valor negativo esto nos empuja al personaje hacia abajo
        //asi le aplicaremos la gravedad
        controller.Move(playerVelocity * Time.deltaTime);
    }
#endregion

#region FuncionCoger
    void PickObjects()
    {
        //Cuando pulsemos la tecla E
        if(Input.GetKeyDown(KeyCode.E))
        {
            //si no llevamos ningun objeto, dentro de nuestro radio hay algun objeto que podamos coger y ese objeto es reccogible
            if(objectToPick != null && pickedObject == null && objectToPick.gameObject.GetComponent<PickableObject>().isPickable == true)
            {
                //a pickedObject le asignamos el objeto que vamos a recoger
                pickedObject = objectToPick;
                //al objeto que vamos a recoger la cambiamos la variable isPicable a false
                pickedObject.GetComponent<PickableObject>().isPickable = false;
                //Hacemos que el objeto sea hijo del empty interaction zone para que el objeto nos siga
                pickedObject.transform.SetParent(interactionZone);
                //modificamos la posicion del objeto para que este en el centro de la interaction zone
                //Esto seria mejor crear otro empty dentro de nuestro personaje en la posicion de la mano y hacer que el objeto se coloque alli
                pickedObject.transform.position = interactionZone.position;
                //Desactivamos la gravedad del objeto para que no se caiga
                pickedObject.GetComponent<Rigidbody>().useGravity = false;
                //convertimos el objeto en kinematic para que no le afecten las fisicas
                pickedObject.GetComponent<Rigidbody>().isKinematic = true;
            }
            //Si llevamos un objeto
            else if(pickedObject != null)
            {
                //Ponemos la variable isPickable del objeto que llevamos en true para que se pueda volver a recoger
                pickedObject.GetComponent<PickableObject>().isPickable = true;
                //Hacemos que deje de ser hijo del personaje para que deje de seguirlo
                pickedObject.transform.SetParent(null);
                //Activamos que le afecte la gravedad
                pickedObject.GetComponent<Rigidbody>().useGravity = true;
                //desactivamos el kinematic para que le vuelvan a afectar las fisicas
                pickedObject.GetComponent<Rigidbody>().isKinematic = false;
                //vaciamos nuestra variable de pickedObject para que podamos recoger otro
                pickedObject = null;
            }
        }
    }
#endregion

    //Funcion que detecta colisiones del Character Controller con objetos que tengan Collider
    private void OnControllerColliderHit(ControllerColliderHit hit) 
    {
        //si tocamos un objeto con el tag de Empujable
        if(hit.gameObject.tag == "Empujable")
        {
            //Creamos una variable temporal para almacenar el Rigidbody del objeto que tocamos
            Rigidbody body = hit.collider.attachedRigidbody;

            //Si el objeto no tiene Rigidbody o es Kinetico dejamos de ejecutar el codigo
            if(body == null || body.isKinematic)
            {
                return;
            }

            //Variable temporal para almacenar la direccion en la que empujaremos el objeto
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);

            //Le añadimos velocidad al Rigidbody del objeto en la direccion deseada
            //Lo multiplicamos por una variable de fuerza para poder modificar la fuerza para empujar cosas del personaje
            //Dividimos por la massa del Rigidbody del objeto para que objetos mas pesado cueste mas moverlos
            body.velocity = pushDir * pushStrength / body.mass;
        }

        //Cuando tocamos la layer del suelo desactivamos la animacion de salto
        if(hit.gameObject.layer == 3)
        {
            anim.SetBool("IsJumping", false);
        }
    }

    //Funcion para dibujar Gizmos
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundSensor.position, sensorRadius);
    }
}