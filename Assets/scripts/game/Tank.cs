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
    const float turnSpeed = 3.5f;

    GameObject _camera;
    GameObject _turret;
	GameObject _nameTag;
    bool _isFiring = false;
    Combat _combat;
    Rigidbody _rigid;
    float _move;
    float _rotation;

    void Awake()
    {
        _turret = transform.GetChild(1).gameObject;
        _rigid = GetComponent<Rigidbody>();
        _combat = GetComponent<Combat>();
        _nameTag = Instantiate(nameTagPrefab, transform.position, Quaternion.identity) as GameObject;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Follow>().targetTrans = transform;
        transform.GetChild(0).gameObject.SetActive(true);
    }

	void Start() {
        foreach (SpriteRenderer r in GetComponentsInChildren<SpriteRenderer>())
		{
			r.material.color = color;
		}
        if (isLocalPlayer)
        {
            _nameTag.GetComponentInChildren<TextMesh>().text = "You";
        }
        else
        {
            _nameTag.GetComponentInChildren<TextMesh>().text = playerName;
        }
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
            StartCoroutine(_combat.Fire(_turret.transform.position + _turret.transform.up * 0.6f, _turret.transform.rotation.eulerAngles));
            _isFiring = false;
        }
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer || _combat.isDead)
        {
            return;
        }
        Quaternion deltaRotation = Quaternion.Euler(0, 0, _rotation);
        _rigid.MoveRotation(_rigid.rotation * deltaRotation);
        _rigid.AddForce(transform.up * _move);
    }

    void Aim()
    {
        Vector3 mouse_pos = Input.mousePosition;
        mouse_pos.z = 0.0f;
        Vector3 object_pos = Camera.main.WorldToScreenPoint(transform.position);
        mouse_pos.x = mouse_pos.x - object_pos.x;
        mouse_pos.y = mouse_pos.y - object_pos.y;
        // -90 because my sprite is aiming up
        float angle = (Mathf.Atan2(mouse_pos.y, mouse_pos.x) * Mathf.Rad2Deg) - 90;
        Vector3 rotationVector = new Vector3(0, 0, angle);
        _turret.transform.rotation = Quaternion.Euler(rotationVector);
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
        if (_move != 0)
        {
            Debug.Log("Trying to move" + _move);
        }

        _rotation = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            _rotation += -turnSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            _rotation += turnSpeed;
        }
        if (speed > 0)
        {
            _rotation *= -1;
        }
    }
}
