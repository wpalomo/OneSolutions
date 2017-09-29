package dev.edmt.ventasapp;

import android.content.Intent;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.ImageButton;

public class MainActivity extends AppCompatActivity {
   ImageButton btnSiguiente;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        btnSiguiente = (ImageButton) findViewById(R.id.btn_settings);

        btnSiguiente.setOnClickListener(new View.OnClickListener()
        {
                 @Override
            public void onClick(View v)
                 {
                     Intent siguiente  = new Intent(MainActivity.this,l_credenciales.class );
                    startActivity(siguiente);
                 }
        });
    }
}
