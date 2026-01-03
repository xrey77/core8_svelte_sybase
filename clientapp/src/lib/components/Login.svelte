<script lang="ts">
  import Mfa from "./Mfa.svelte";
  import type { UIEventHandler } from 'svelte/elements';
  import axios from 'axios';
  import jQuery from 'jquery';  
  // export let form: ActionData;  
  let message: string = "";
  let isdisable: boolean = false;

  const api = axios.create({
    baseURL: "https://localhost:7286",
    headers: {'Accept': 'application/json',
              'Content-Type': 'application/json'}
  });

  function CloseLogin(event: any) {
    event.preventDefault();
    message = "";
    jQuery("#loginReset").trigger("click");
    window.location.reload();
  }

  const SubmitLogin: SubmitEventHandler<SubmitEvent, HTMLFormElement> = (event: any) => {  
    event.preventDefault();
    isdisable = true;
    const formData = new FormData(event.currentTarget);
    const data = Object.fromEntries(formData.entries());    
    const jsondata = JSON.stringify({ username: data.username, password_hash: data.password });
        api.post("/signin", jsondata)
        .then((res: any) => {
                message = res.data.message;
                if (res.data.qrcodeurl !== '/images/qrcode.png') {
                    sessionStorage.setItem('USERID',res.data.id);
                    sessionStorage.setItem('TOKEN',res.data.token);
                    sessionStorage.setItem('ROLE',res.data.roles);
                    sessionStorage.setItem('USERPIC',res.data.profilepic);
                    jQuery("#loginReset").trigger("click");
                    jQuery("#mfaModal").trigger("click");
                } else {
                    sessionStorage.setItem('USERID',res.data.id);
                    sessionStorage.setItem('USERNAME',res.data.username);
                    sessionStorage.setItem('TOKEN',res.data.token);                        
                    sessionStorage.setItem('ROLE',res.data.roles);
                    sessionStorage.setItem('USERPIC',res.data.profilepic);                    

                    window.setTimeout(() => {
                      CloseLogin;
                      message = '';                    
                      isdisable = false;
                      window.location.reload();
                }, 3000);

                }
          }, (error: any) => {
                if (error.response) {
                  message = error.response.data.message;
                } else {
                  message = error.message;
                }
                window.setTimeout(() => {
                    message = '';
                    isdisable = false;
                }, 3000);
                return;
        });   

  };

</script>
<div class="modal fade" id="staticLogin" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="staticLoginLabel" aria-hidden="true">
  <div class="modal-dialog modal-sm modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header bg-danger">
        <h1 class="modal-title fs-5 text-white" id="staticLoginLabel">User Login</h1>
        <button on:click={CloseLogin} id="closeLogin" type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
      </div>
      <div class="modal-body">
        <form on:submit={SubmitLogin} autocomplete="off">
            <div class="mb-3">
              <input type="text" required class="form-control" id="username" name="username" placeholder="enter Username" disabled={isdisable}>
            </div>            
            <div class="mb-3">
                <input type="password" required class="form-control" id="password" name="password" placeholder="enter Password" disabled={isdisable}>
            </div>              
            <button type="submit" class="btn btn-danger" disabled={isdisable}>login</button>
            <button id="loginReset" type="reset" class="btn btn-danger">reset</button>
            <button id="mfaModal" type="reset" class="btn d-none" data-bs-toggle="modal" data-bs-target="#staticMfa">mfa</button>
        </form>
      </div>
      <div class="modal-footer">

        <div class="w-100 text-center text-danger">{message as string}</div> 
      </div>
    </div>
  </div>
</div>
<Mfa/>

