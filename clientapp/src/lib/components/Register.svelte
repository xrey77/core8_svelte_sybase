<script lang="ts">
  import type { UIEventHandler } from 'svelte/elements';
  import axios from 'axios';
  let message: string = '';
  // export let form: ActionData;
  const api = axios.create({
    baseURL: "https://localhost:7286",
    headers: {'Accept': 'application/json',
              'Content-Type': 'application/json'}
  });

  function CloseRegistration(event: any) {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const data = Object.fromEntries(formData.entries());
    data.firstname = "";
    data.lastname = "";
    data.email = "";
    data.mobile = "";
    data.usrname = ""
    data.passwrd = "";
  }

  const SubmitRegistration: UIEventHandler<SubmitEvent, HTMLFormElement> = async (event: any) => {
    event.preventDefault();
    message = "Please wait....";
    const formData = new FormData(event.currentTarget);
    const data = Object.fromEntries(formData.entries());
    const jsondata = JSON.stringify({firstname: data.firstname, lastname: data.lastname, email: data.email, mobile: data.mobile, username: data.usrname, password_hash: data.passwrd });
    await api.post("/signup", jsondata)
        .then((res: any) => {
                message = res.data.message;
                
          }, (error: any) => {
                message = error.response.data.message;
                window.setTimeout(() => {
                    message = '';
                }, 3000);
                return;
          });   
    };

</script>

<div class="modal fade" id="staticRegister" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="staticRegisterLabel" aria-hidden="true">
  <div class="modal-dialog modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header bg-primary">
        <h1 class="modal-title fs-5 text-white" id="staticRegisterLabel">Account Registration</h1>
        <button on:click={CloseRegistration} type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
      </div>
      <div class="modal-body">
        <form on:submit={SubmitRegistration} autocomplete="off" >
            <div class="row">
                <div class="col">
                    <div class="mb-3">
                        <input type="text" required class="form-control" id="firstname" name="firstname" placeholder="enter Firstname">
                    </div>                      
                </div>
                <div class="col">
                    <div class="mb-3">
                        <input type="text" required class="form-control" id="lastname" name="lastname" placeholder="enter Lastname">
                    </div>                      
                </div>
            </div>

            <div class="row">
                <div class="col">
                    <div class="mb-3">
                        <input type="text" required class="form-control" id="email" name="email" placeholder="enter Email Address">
                    </div>                      
                </div>
                <div class="col">
                    <div class="mb-3">
                        <input type="text" required class="form-control" id="mobile" name="mobile" placeholder="enter Mobile No.">
                    </div>                      
                </div>
            </div>

            <div class="row">
                <div class="col">
                    <div class="mb-3">
                        <input type="text" required class="form-control" id="usrname" name="usrname" placeholder="enter Username">
                    </div>                      
                </div>
                <div class="col">
                    <div class="mb-3">
                        <input type="password" required class="form-control" id="passwrd" name="passwrd" placeholder="enter Password">
                    </div>                      
                </div>
            </div>            
            <button type="submit" class="btn btn-primary">register</button>
            <button type="reset" class="btn btn-primary">reset</button>
        </form>
      </div>
      <div class="modal-footer">
        <div class="w-100 text-center text-danger">{message as string}</div>
        
      </div>
    </div>
  </div>
</div>