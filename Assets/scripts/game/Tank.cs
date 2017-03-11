using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Tank : NetworkBehaviour {
    [SyncVar]
	public string playerName;
	[SyncVar]
    public Color color;

    public GameObject nameTagPrefab;
    public GameObject smokePrefab;

    const float speed = 20f;
    const float turnSpeed = 5f;

    Transform _model;
    Transform _turret;
    Transform _barrel;
	Transform _nameTag;
    bool _isFiring = false;
    Combat _combat;
    Rigidbody _rigid;
    float _move;
    float _rotation;

    void Awake()
    {
        _model = transform.Find("Model");
        _turret = _model.Find("TankTurret");
        _barrel = _turret.Find("Barrel");
        _rigid = GetComponent<Rigidbody>();
        _combat = GetComponent<Combat>();
        _nameTag = transform.Find("Name");
    }

	void Start() {
        foreach (Renderer r in _model.GetComponentsInChildren<Renderer>())
		{
			r.material.color = color;
		}
        _nameTag.GetComponentInChildren<TextMesh>().text = playerName;
        _nameTag.GetComponentInChildren<MeshRenderer>().enabled = true;
        _nameTag.GetComponentInChildren<Renderer>().sortingLayerName = "Player";
        ShowNameTags();
        StartCoroutine("FadeOut");
    }
    
    void ShowNameTags()
    {
        var renderer = _nameTag.GetComponentInChildren<Renderer>();
        var c = renderer.material.color;
        c.a = 1f;
        renderer.material.color = c;
    }

	IEnumerator FadeOut() {
		var renderer = _nameTag.GetComponentInChildren<Renderer> ();
		var opacity = 1f;
		while (opacity > 0) {
			yield return new WaitForSeconds (0.1f);
			opacity -= 0.05f;
			var c = renderer.material.color;
			c.a = opacity;
			renderer.material.color = c;
		}
	}

    void OnGUI()
    {
         _nameTag.transform.position = transform.position;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            StopCoroutine("FadeOut");
            ShowNameTags();
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            StartCoroutine("FadeOut");
        }
        if (!isLocalPlayer || _combat.isDead)
        {
            return;
        }
        Move();
        Aim();
        if (Input.GetMouseButton(0) && !_isFiring)
        {
            _isFiring = true;
            StartCoroutine(_combat.Fire(_barrel));
            _isFiring = false;
        }
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer || _combat.isDead)
        {
            return;
        }
        Quaternion deltaRotation = Quaternion.Euler(0, _rotation, 0);
        _rigid.MoveRotation(_rigid.rotation * deltaRotation);
        _rigid.AddForce(transform.forward * _move);
    }

    void Aim()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            Vector3 target = ray.GetPoint(distance);
            Vector3 direction = target - _turret.transform.position;
            float rotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            _turret.rotation = Quaternion.Euler(0, rotation, 0);
        }
    }

    void Move()
    {
		if (_isFiring) {
			return;
		}

        _move = 0;
        if(Input.GetKey(KeyCode.W))
        {
            _move = speed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            _move = -speed;
        }

        _rotation = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            _rotation += turnSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            _rotation += -turnSpeed;
        }
        if (speed > 0)
        {
            _rotation *= -1;
        }
    }
}
