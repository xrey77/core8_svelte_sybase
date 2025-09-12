<script lang="ts">
  import type { UIEventHandler } from 'svelte/elements';
   import axios from 'axios';
   import { onMount } from 'svelte';
   import jQuery from 'jquery';

   let userid: string = '';
   let firstname: string = '';
   let lastname: string = '';
   let email: string = '';
   let mobile: string = '';
   let message: string = '';
   let showSave: boolean =false;
   let profilepic: string = '';
   let chgPwd: boolean = false;
   let chkMfa: boolean = false;
   let qrcodeurl: string = '';
   let token: string = '';
   let selectedFile: File | null = null;
   let newpassword: string = '';
   let confnewpassword: string = '';
   let cbpwd: any = null;
   let cbmfa: any = null;

  const api = axios.create({
    baseURL: "https://localhost:7286",
    headers: {'Accept': 'application/json',
              'Content-Type': 'application/json'}
  });

  onMount(async () => {

    const uid = sessionStorage.getItem('USERID');
    if (uid != undefined) {
      userid = uid;
    } else {
      userid = '';
    }

    const tken = sessionStorage.getItem('TOKEN');
    if (tken != undefined) {
        token = tken;
    } else {
        token = '';
    }
    jQuery('#chkPwd').prop('checked', false);
    jQuery('#chkMfa').prop('checked', false);
    chgPwd = false;
    chkMfa = false;
    message = "Please wait.....";
    api.get(`/api/getbyid/${userid}`, { headers: {
        Authorization: `Bearer ${token}`
    }} )
        .then((res: any) => {
                message = res.data.user.message;
                firstname = res.data.user.firstname;
                lastname = res.data.user.lastname;
                email = res.data.user.email;
                mobile = res.data.user.mobile; 
                profilepic = res.data.user.profilepic;
                if (res.data.user.qrcodeurl === '') {
                  qrcodeurl = '';
                } else {
                  qrcodeurl = res.data.user.qrcodeurl;
                }
          }, (error: any) => {
              message = error.response.data.message;
        });    
        window.setTimeout(() => {
          message = '';
        }, 3000);  
  });  

  const submitProfile: SubmitEventHandler<SubmitEvent, HTMLFormElement> = (event: any) => {  
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const data = Object.fromEntries(formData.entries());    


    const jdata =JSON.stringify({ lastname: data.lastname, 
        firstname: data.firstname, mobile: data.mobile });

    api.patch(`/api/updateprofile/${userid}`, jdata, { headers: {
    Authorization: `Bearer ${token}`
    }})
    .then((res: any) => {
            message = res.data.message;
      }, (error: any) => {
            message = error.response.data.message;
    });
    window.setTimeout(() => {
      message = '';
    }, 3000);    
  };

  function changePicture(event: any) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      selectedFile = input.files[0];
    }

    if (selectedFile) {
        let formdata = new FormData();
        formdata.append('Id', userid);
        formdata.append('Profilepic', selectedFile);
        api.post("/api/uploadpicture", formdata, { headers: {
        'Content-Type': 'multipart/form-data',
        Authorization: `Bearer ${token}`
        }} )
        .then((res: any) => {
            message = res.data.message;
            profilepic = res.data.profilepic;
            sessionStorage.setItem('USERPIC',res.data.profilepic);
            window.setTimeout(() => {
              message = '';
              window.location.reload();

            }, 3000);    
        }, (error: any) => {
              message = error.response.data.message;
        });
        window.setTimeout(() => {
          message = '';
        }, 3000);
    } 
  }

  function checkboxPassword(e: any) {
    if (e.target.checked) {
        chgPwd = true
        cbpwd = 'checked';
        cbmfa = null;
        chkMfa = false;
        showSave = false;
        jQuery('#chkMfa').prop('checked', false);
    } else {
        chgPwd = false;
        cbpwd = null;
        cbmfa = null;
        chkMfa = false;
        jQuery("#newpassword").val('');
        jQuery("#confnewpassword").val('');
    }
  }

  function checkboxMFA(event: any) {
    if (event.target.checked) {
      chkMfa = true;
      cbmfa = 'checked';
      chgPwd = false;
      cbpwd = '';
      showSave = true;
      jQuery('#chkPwd').prop('checked', false);
    } else {
        chkMfa = false;
        cbmfa = ''
        cbpwd = '';
        chgPwd =false;
        showSave = false;
    }
  }

    const changePassword: SubmitEventHandler<SubmitEvent, HTMLFormElement> = (event: any) => {  
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const jdata = Object.fromEntries(formData.entries());    

    event.preventDefault();
    if (jdata.newpassword === '') {
        message = "Please enter New Password.";
        setTimeout(() => {
          message = '';
        }, 3000);
        return;
    }
    if (jdata.confnewpassword === '') {
        message = "Please confirm New Password.";
        setTimeout(() => {
          message = '';
        }, 3000);
        return;
    }
    if (jdata.newpassword != jdata.confnewpassword) {
        message = "New Password does not matched.";
        setTimeout(() => {
          message = '';
          jQuery("#newpassword").val('');
          jQuery("#confnewpassword").val('');
        }, 3000);
        return;
    }
    message = 'Please wait...';
    const data =JSON.stringify({ password_hash: jdata.newpassword });
    api.patch(`/api/updatepassword/${userid}`, data, { headers: {
    Authorization: `Bearer ${token}`
    }} )
    .then((res: any) => {
          message = res.data.message;
      }, (error: any) => {
            message = error.response.data.message;
    });
    window.setTimeout(() => {
      message = '';
      newpassword = '';
      confnewpassword = '';
    }, 3000);
  }

   async function enableMFA(e: any) {
    e.preventDefault();
    const data =JSON.stringify({ Twofactorenabled: true });
    await api.patch(`/api/enablemfa/${userid}`, data, { headers: {
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token}`
    }} )
    .then((res: any) => {
        message = res.data.message;
        window.setTimeout(() => {
            message = '';
            qrcodeurl = res.data.qrcodeurl;
        }, 3000);

      }, (error: any) => {
        message = error.response.data.message;
        window.setTimeout(() => {
          message = '';
        }, 3000);
    });
  }

  async function disableMFA(e: Event) {
    e.preventDefault();
    const data =JSON.stringify({ Twofactorenabled: false });
    await api.patch(`/api/enablemfa/${userid}`, data, { headers: {
        Authorization: `Bearer ${token}`
    }} )
    .then((res: any) => {
      message = res.data.message;
      window.setTimeout(() => {
          qrcodeurl = '';
          qrcodeurl = res.data.qrcodeurl;
          message = '';
          window.location.reload();
        }, 3000);

      }, (error: any) => {
        message = error.response.data.message;
        window.setTimeout(() => {
          message = '';
        }, 3000);
    });
  }
</script>
<div class="container">
<div class="card card-width bs-info-border-subtle">
    <div class="card-header bg-info text-light">
      <strong>USER'S PROFILE NO.&nbsp; {userid}</strong>
    </div>
    <div class="card-body">

      <form onsubmit={submitProfile} enctype='multipart/form-data' autocomplete='off'>
          <div class="row">
              <div class="col">
                  <div class="mb-3">
                      <input type="text" id="firstname" name="firstname" required value={firstname} class="form-control"  autocomplete="off"/>
                  </div>
                  <div class="mb-3">
                      <input type="text" required id="lastname" name="lastname" value={lastname} class="form-control" autocomplete="off"/>
                  </div>
                  <div class="mb-3">
                      <input type="email" id="emal" name="email" value={email} class="form-control" readonly/>
                  </div>
                  <div class="mb-3">
                      <input type="text" required id="mobile" name="mobile" value={mobile} class="form-control" autocomplete="off"/>
                  </div>
                  <div class="mb-3">
                    {#if showSave == false}                    
                      <button type="submit" class="btn btn-info">save</button>
                    {/if}
                  </div>

              </div>
              <div class="col">
                  <img id="userpic" class="usr" src={profilepic as string} alt=""/>
                  <div class="mb-3">
                      <input type="file" multiple accept="image/*" onchange={changePicture} class="form-control form-control-sm mt-3"/>
                  </div>

              </div>
              <div class="mb-3">
            </div>

          </div>
        </form>

          <div class="row">
              <div class="col">
                  <div class="form-check">
                      <input id="chkPwd" onchange={checkboxPassword} type="checkbox" class="form-check-input bcolor" />
                      <label class="form-check-label" for="chgPwd">
                          Change Password
                      </label>
                  </div>
              </div>
              <div class="col">
                  <div class="form-check">
                      <input class="form-check-input bcolor" type="checkbox" id="chkMfa"  onchange={checkboxMFA} />
                      <label class="form-check-label" for="chkMfa">
                          Multi-Factor Authenticator
                      </label>
                  </div>
              </div>
          </div>

          <div class="row">

              <div class="col">

                {#if chgPwd}
                  <form onsubmit={changePassword} autocomplete="off">
                  <div class="mb-3">
                      <input type="password" id="newpassword" name="newpassword" class="form-control pwd mt-2" placeholder="enter new Password" autocomplete="off"/>
                  </div>
                  <div class="mb-3">
                      <input type="password" id="confnewpassword" name="confnewpassword" class="form-control pwd" placeholder="confirm new Password" autocomplete="off"/>
                  </div>
                  <button type="submit" class="btn btn-primary">change password</button>
                </form>
                {/if}

                {#if chkMfa}
                    <img class="qrcode1" src={qrcodeurl} alt=""/>
                {/if}
              </div>

              <div class="col">
                  {#if chkMfa == true}
                          <p id="qrcode-cap1" class='text-danger'><strong>Requirements</strong></p>
                          <p id="qrcode-cap2">You need to install <strong>Google or Microsoft Authenticator</strong> in your Mobile Phone, once installed, click Enable Button below, and <strong>SCAN QR CODE</strong>, next time you login, another dialog window will appear, then enter the <strong>OTP CODE</strong> from your Mobile Phone in order for you to login.</p>
                          <div class="row">
                              <div class="col">
                                  <button onclick={enableMFA} type="button" class="btn btn-primary qrcode-cap3 mx-2">Enable</button>
                                  <button onclick={disableMFA} type="button" class="btn btn-secondary qrcode-cap3">Disable</button>
                                </div>
                          </div>
                  {/if}
              </div>
          </div>
    </div>

    {#if message != ''}        
     <div class="card-footer">
       <div class="w-100 text-left text-danger">{message}</div>
     </div>
    {/if}

  </div>    
</div>