<script lang="ts">
  import type { UIEventHandler } from 'svelte/elements';
  // export let form: ActionData;
  import { onMount } from 'svelte';
  import axios from 'axios'

  const api = axios.create({
  baseURL: "https://localhost:7286",
  headers: {'Accept': 'application/json',
            'Content-Type': 'application/json'}
  });

  let message: string = '';
  let otp: string = '';
  let userid: string = '';

  onMount(async () => {
    const uid = sessionStorage.getItem('USERID');
    if (uid !== null) {
      userid = uid;
    } else {
      userid = '';
    }
  });

  const closeMFA = () => {
        sessionStorage.removeItem('USERID');
        sessionStorage.removeItem('USERNAME');
        sessionStorage.removeItem('TOKEN');
        sessionStorage.removeItem('USERPIC');
        otp = '';
        message = '';
    }

  const SubmitMfa: SubmitEventHandler<SubmitEvent, HTMLFormElement> = (event: any) => {    event.preventDefault();
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const jdata = Object.fromEntries(formData.entries());    

    const data =JSON.stringify({ id: userid, otp: jdata.otp });
        api.post("/validateotp", data)
        .then((res: any) => {
                message = res.data.message;
                sessionStorage.setItem("USERNAME", res.data.username);
                window.setTimeout(() => {
                  otp = '';
                  message = '';
                  window.location.reload();
                },3000);
          }, (error: any) => {
            if (error.response) {
              message = error.response.data.message;
            } else {
              message = error.message;
            }
            window.setTimeout(() => {
              message = '';
              otp = '';
            }, 3000);    
            return;
        });            
  };

</script>

<div class="modal fade" id="staticMfa" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="staticMfaLabel" aria-hidden="true">
  <div class="modal-dialog modal-sm modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header bg-warning">
        <h1 class="modal-title fs-5" id="staticMfaLabel">Multi-Factor Auth</h1>
        <button onclick={closeMFA} type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
      </div>
      <div class="modal-body">
        <form onsubmit={SubmitMfa} autocomplete="off">
            <div class="mb-3">
                <input type="text" required class="form-control" id="otp" name="otp" placeholder="enter 6-digit OTP">
            </div>                      
            <button type="submit" class="btn btn-warning">submit</button>
            <button type="reset" class="btn btn-warning">reset</button>
        </form>
      </div>
      <div class="modal-footer">
        <div class="w-100 text-center text-danger">{message as string}</div>
      </div>
    </div>
  </div>
</div>